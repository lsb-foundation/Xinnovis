﻿using CommonLib.Extensions;
using CommonLib.Models;
using MFCSoftware.Common;
using MFCSoftware.Models;
using MFCSoftware.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using System.Globalization;
using System.Windows.Media;
using System.Threading.Tasks;
using CommonLib.Utils;
using MFCSoftware.Utils;
using System.Text;

namespace MFCSoftware.Views
{
    /// <summary>
    /// ChannelUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelUserControl : UserControl
    {
        private readonly ChannelUserControlViewModel _viewModel;
        private readonly FlowDataSaver _flowDataSaver;

        public ChannelUserControl(int address)
        {
            InitializeComponent();
            _viewModel = this.DataContext as ChannelUserControlViewModel;
            _flowDataSaver = new FlowDataSaver { Interval = _viewModel.InsertInterval };
            _viewModel.Address = address;
            _viewModel.InsertIntervalChanged += interval => _flowDataSaver.Interval = interval;
        }

        public event Action<ChannelUserControl> ChannelClosed; //控件被移除
        public event Action<ChannelUserControl, SerialCommand<byte[]>> SingleCommandSended;    //单指令发送

        public int Address { get => _viewModel.Address; }
        public SerialCommand<byte[]> ReadFlowBytes { get => _viewModel.ReadFlowBytes; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get => _viewModel.ReadBaseInfoBytes; }
        public SerialCommand<byte[]> ReadVersion { get => _viewModel.ReadVersion; }

        private void Closed(object sender, RoutedEventArgs e)
        {
            ChannelClosed?.Invoke(this);
        }

        public void ResolveData(byte[] data, SerialCommandType type)
        {
            this.Dispatcher.Invoke(async () =>
            {
                try
                {
                    LogHelper.WriteRecievedHexData(data, type);

                    if (!data.CheckCRC16ByDefault())
                        throw new Exception("CRC校验失败。");

                    switch (type)
                    {
                        case SerialCommandType.BaseInfoData:
                            HandleBaseInformation(data);
                            break;
                        case SerialCommandType.ReadVersion:
                            HandleVersionData(data);
                            break;
                        case SerialCommandType.ReadFlow:
                            //var flow = HandleFlowData(data);
                            var flow = FlowData.ResolveFromBytes(data, AppSettings.ReadTemperature);
                            _viewModel.SetFlow(flow);
                            _viewModel.UpdateSeries(flow.CurrentFlow);
                            await _flowDataSaver.InsertFlowAsync(flow);
                            break;
                        case SerialCommandType.ClearAccuFlowData:
                        case SerialCommandType.SetFlow:
                        case SerialCommandType.ValveControl:
                        case SerialCommandType.ZeroPointCalibration:
                        case SerialCommandType.FactoryRecovery:
                            _viewModel.ShowMessage(ResolveActionAttribute.CheckAutomatically(data, type));
                            break;
                    }
                }
                catch(Exception e)
                {
                    _viewModel.WhenResolveFailed();
                    _viewModel.ShowMessage(e.Message);
                    LoggerHelper.WriteLog(e.Message, e);
                }
            });
        }

        private void HandleBaseInformation(byte[] data)
        {
            int gas = data.SubArray(3, 2).ToInt32(0, 2);
            int range = data.SubArray(5, 2).ToInt32(0, 2);
            int unit = data.SubArray(7, 2).ToInt32(0, 2);
            string sn = data.SubArray(23, 12).ToHexString();

            BaseInformation info = new()
            {
                SN = sn,
                Range = range,
                GasType = GasTypeCode.GetGasTypeCodesFromConfiguration()?.FirstOrDefault(c => c.Code == gas),
                Unit = UnitCode.GetUnitCodesFromConfiguration()?.FirstOrDefault(u => u.Code == unit)
            };
            _viewModel.SetBaseInfomation(info);
        }

        private void HandleVersionData(byte[] data)
        {
            var version = new DeviceVersion();
            var originModel = Encoding.ASCII.GetString(data.SubArray(3, 10));
            var index = originModel.IndexOf('\0');
            version.Model = index > -1 ? originModel.Substring(0, index) : originModel;
            var originVersion = Encoding.ASCII.GetString(data.SubArray(13, 10));
            index = originVersion.IndexOf('\0');
            version.Version = index > -1 ? originVersion.Substring(0, index) : originVersion;
            _viewModel.SetVersion(version);
        }
        
        private FlowData HandleFlowData(byte[] data)
        {
            Span<byte> dataSpan = data.AsSpan();

            var pointBit = dataSpan.Slice(3, 2).ToInt32(); //小数位数

            var temperatureSpan = dataSpan.Slice(5, 2);
            temperatureSpan.Reverse();
            var temperature = BitConverter.ToInt16(temperatureSpan.ToArray(), 0) / 10.0f;

            float flow = pointBit switch
            {
                0 => dataSpan.Slice(7, 4).ToInt32() * 0.01f,    //两位小数
                1 => dataSpan.Slice(7, 4).ToInt32() * 0.001f,   //三位小数
                2 => dataSpan.Slice(7, 4).ToInt32(),            //整数
                3 => dataSpan.Slice(7, 4).ToInt32() * 0.1f,     //一位小数
                _ => 0
            };

            Span<byte> accuSpan = dataSpan.Slice(11, 8);
            accuSpan.Reverse();
            float accuFlow = BitConverter.ToInt64(accuSpan.ToArray(), 0) / 1000.0f;

            int unitCode = dataSpan.Slice(19, 2).ToInt32();
            int days = dataSpan.Slice(21, 2).ToInt32();
            int hours = dataSpan.Slice(23, 2).ToInt32();
            int mins = dataSpan.Slice(25, 2).ToInt32();
            int secs = dataSpan.Slice(27, 2).ToInt32();
            
            var flowData = new FlowData()
            {
                Address = Address,
                Unit = _viewModel.BaseInfo?.Unit?.Unit,
                CollectTime = DateTime.Now,
                CurrentFlow = flow,
                AccuFlow = accuFlow,
                Temperature = temperature,
                AccuFlowUnit = unitCode == 0 ? "L" : (unitCode == 1 ? "m³" : string.Empty),
                Days = days,
                Hours = hours,
                Minutes = mins,
                Seconds = secs
            };

            _viewModel.SetFlow(flowData, pointBit);
            _viewModel.UpdateSeries(flow);
            return flowData;
        }

        public void WhenTimeOut()
        {
            this.Dispatcher.Invoke(() => _viewModel.WhenTimeOut());
        }

        //递归查找当前控件的根节点Window对象
        private DependencyObject GetCurrentWindow(DependencyObject obj)
        {
            if (obj is Window) return obj;
            return GetCurrentWindow(VisualTreeHelper.GetParent(obj));
        }

        private async void ExportFlowButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new ExportSelectWindow
            {
                Owner = GetCurrentWindow(this) as Window,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false
            };
            _ = win.ShowDialog();

            if (win.IsReady)
            {
                var flowDatas = win.ExportType == ExportType.ByTime ?
                    await SqliteHelper.QueryFlowDatasByTimeAsync(win.FromTime, win.ToTime, Address) :
                    await SqliteHelper.QueryAllFlowDatasAsync(Address);

                if (flowDatas?.Count > 0)
                {
                    var dialog = new SaveFileDialog()
                    {
                        Filter = "Excel文件|*.xlsx;*.xls",
                        Title = "导出数据"
                    };

                    if ((bool)dialog.ShowDialog())
                    {
                        if (!string.IsNullOrEmpty(dialog.FileName))
                        {
                            await Task.Run(() => FlowData.ExportFlowDatas(dialog.FileName, flowDatas));
                            _viewModel.ShowMessage("导出完成。");
                        }
                    }
                }
                else
                {
                    _viewModel.ShowMessage("未查询到数据！");
                }
            }
        }

        private void ClearAccuFlowButton_Click(object sender, RoutedEventArgs e)
        {
            if (SendSingleCommandConfirm(sender))
            {
                SingleCommandSended?.Invoke(this, _viewModel.ClearAccuFlowBytes);
            }
        }

        private void ControlButton_Clicked(object sender, RoutedEventArgs e)
        {
            if(_viewModel.Selector == ControlSelector.FlowValue)
            {
                CheckFlowValueAndSendCommand();
            }
            else if(_viewModel.Selector == ControlSelector.ValveOpenValue)
            {
                CheckValveOpenValueAndSendCommand();
            }
        }

        private void CheckFlowValueAndSendCommand()
        {
            if (_viewModel.BaseInfo == null)
            {
                _viewModel.ShowMessage("未获取到基础数据，量程未知。");
                return;
            }
            if (_viewModel.FlowValue < 0 || _viewModel.FlowValue > _viewModel.BaseInfo.Range)
            {
                _viewModel.ShowMessage("流量数据必须大于等于0，小于等于量程。");
                return;
            }
            if (!AppSettings.AllowLowerFlowValue)
            {
                if (_viewModel.FlowValue < _viewModel.BaseInfo.Range * 0.01F &&
                    _viewModel.FlowValue != 0)  //允许等于0
                {
                    _viewModel.ShowMessage("设定值不能小于满量程的1%");
                    return;
                }
            }
            _viewModel.SetWriteFlowBytes();
            SingleCommandSended?.Invoke(this, _viewModel.WriteFlowBytes);
        }

        private void CheckValveOpenValueAndSendCommand()
        {
            if(_viewModel.ValveOpenValue < 0 || _viewModel.ValveOpenValue > 100)
            {
                _viewModel.ShowMessage("阀门开度必须大于等于0，小于等于100。");
                return;
            }
            _viewModel.SetWriteValveBytes();
            SingleCommandSended?.Invoke(this, _viewModel.WriteValveBytes);
        }

        private void SetSaveTimeButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(saveIntervalTextBox.Text, out uint _))
                _viewModel.ShowMessage("保存时间间隔设置成功。");
            else _viewModel.ShowMessage("输入有误，请重新输入。");
        }

        private void ZeroPointCaliButton_Click(object sender, RoutedEventArgs e)
        {
            if (SendSingleCommandConfirm(sender))
            {
                SingleCommandSended?.Invoke(this, _viewModel.ZeroPointCalibrationBytes);
            }
        }

        private void FactoryRecoveryButton_Click(object sender, RoutedEventArgs e)
        {
            if (SendSingleCommandConfirm(sender))
            {
                SingleCommandSended?.Invoke(this, _viewModel.FactoryRecoveryBytes);
            }
        }

        private bool SendSingleCommandConfirm(object sender)
        {
            var buttonText = (sender as Button).Content as string;
            return MessageBox.Show($"{buttonText}确认?", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            ControlButton_Clicked(sender, e);
        }
    }

    /// <summary>
    /// Xaml中FlowData累积时间的转换器
    /// </summary>
    public class FlowDataToTimeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timeText = string.Empty;
            if (value is FlowData flowData)
            {
                timeText = $"{flowData.Days}:{flowData.Hours}:{flowData.Minutes}:{flowData.Seconds}";
            }
            return timeText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Xaml中绑定枚举到布尔类型的转换器
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);
            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
