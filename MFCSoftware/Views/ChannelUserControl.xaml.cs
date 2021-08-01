using CommonLib.Extensions;
using CommonLib.Models;
using MFCSoftware.Common;
using MFCSoftware.Models;
using MFCSoftware.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.Windows.Media;
using System.Threading.Tasks;
using CommonLib.Utils;
using CommonLib.MfcUtils;

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
            _flowDataSaver = new FlowDataSaver(address, (int)_viewModel.InsertInterval);
            _viewModel.Address = address;
            _viewModel.InsertIntervalChanged += seconds => _flowDataSaver.SetInterval((int)seconds);
        }

        public event Action<ChannelUserControl> ChannelClosed; //控件被移除
        public event Action<ChannelUserControl, SerialCommand<byte[]>> SingleCommandSended;    //单指令发送

        public int Address { get => _viewModel.Address; }
        public SerialCommand<byte[]> ReadFlowBytes { get => _viewModel.ReadFlowBytes; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get => _viewModel.ReadBaseInfoBytes; }

        private void Closed(object sender, RoutedEventArgs e)
        {
            ChannelClosed?.Invoke(this);
        }

        public void ResolveData(byte[] data, SerialCommandType type)
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    LogHelper.WriteRecievedHexData(data, type);

                    if (!data.CheckCRC16ByDefault())
                        throw new Exception("CRC校验失败。");

                    switch (type)
                    {
                        case SerialCommandType.BaseInfoData:
                            ResolveBaseInfoData(data);
                            break;
                        case SerialCommandType.ReadFlow:
                            ResolveFlowData(data);
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

        private void ResolveBaseInfoData(byte[] data)
        {
            int gas = data.SubArray(3, 2).ToInt32(0, 2);
            int range = data.SubArray(5, 2).ToInt32(0, 2);
            int unit = data.SubArray(7, 2).ToInt32(0, 2);
            string sn = data.SubArray(23, 12).ToHexString();

            BaseInformation info = new BaseInformation()
            {
                SN = sn,
                Range = range,
                GasType = GasTypeCode.GetGasTypeCodesFromConfiguration()?.FirstOrDefault(c => c.Code == gas),
                Unit = UnitCode.GetUnitCodesFromConfiguration()?.FirstOrDefault(u => u.Code == unit)
            };
            _viewModel.SetBaseInfomation(info);
        }

        private void ResolveFlowData(byte[] data)
        {
            //addr 0x03 0x16 FLOW1 FLOW2 FLOW3 FLOW4
            //ACCMULATE1 ACCMULATE2 ACCMULATE3 ACCMULATE4 ACCMULATE5 ACCMULATE6 ACCMULATE7 ACCMULATE8
            //UNIT1 UNIT2 DAY1 DAY2 HOUR1 HOUR2 MIN1 MIN2 SEC1 SEC2 CRCL CRCH
            Span<byte> dataSpan = data.AsSpan();
            float flow = dataSpan.Slice(3, 4).ToInt32ForHighFirst() / 100.0f;
            Span<byte> accuFlowSpan = dataSpan.Slice(7, 8);
            accuFlowSpan.Reverse();
            float accuFlow = BitConverter.ToInt64(accuFlowSpan.ToArray(), 0) / 1000.0f;
            int unitCode = dataSpan.Slice(15, 2).ToInt32ForHighFirst();

            string unit = string.Empty;
            if (unitCode == 0)
            {
                unit = "L";
            }
            else if (unitCode == 1)
            {
                unit = "m³";
            }

            int days = dataSpan.Slice(17, 2).ToInt32ForHighFirst();
            int hours = dataSpan.Slice(19, 2).ToInt32ForHighFirst();
            int mins = dataSpan.Slice(21, 2).ToInt32ForHighFirst();
            int secs = dataSpan.Slice(23, 2).ToInt32ForHighFirst();

            FlowData flowData = new FlowData()
            {
                CurrentFlow = flow,
                Unit = _viewModel.BaseInfo?.Unit?.Unit,
                AccuFlow = accuFlow,
                AccuFlowUnit = unit,
                Days = days,
                Hours = hours,
                Minutes = mins,
                Seconds = secs
            };
            _viewModel.SetFlow(flowData);
            _viewModel.UpdateSeries();
            _flowDataSaver.Flow = flowData;
        }

        public void WhenTimeOut()
        {
            this.Dispatcher.Invoke(() =>
            {
                _viewModel.WhenTimeOut();
#if DEBUG
                Console.WriteLine($"Channel {Address}: TimeoutException");
#endif
            });
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
            win.ShowDialog();

            if(win.IsReady)
            {
                var flowDatas = win.ExportType == ExportType.ByTime ?
                    await DbStorage.QueryFlowDatasByTimeAsync(win.FromTime, win.ToTime, Address) :
                    await DbStorage.QueryAllFlowDatasAsync(Address);

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
                            ExportHistoryFlowDataToExcel(dialog.FileName, flowDatas);
                        }
                    }
                }
                else
                {
                    _viewModel.ShowMessage("未查询到数据！");
                }
            }
        }

        public async void ExportHistoryFlowDataToExcel(string fileName, List<FlowData> flowDatas)
        {
            try
            {
                await Task.Run(async () =>
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet sheet;
                        if (package.Workbook.Worksheets.Any(e => e.Name == "流量数据表"))
                            sheet = package.Workbook.Worksheets.FirstOrDefault(e => e.Name == "流量数据表");
                        else sheet = package.Workbook.Worksheets.Add("流量数据表");

                        //Epplus操作Excel从1开始
                        for (int column = 1; column <= 5; column++)
                        {
                            sheet.Cells[1, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        sheet.SetValue(1, 1, "采样时间");
                        sheet.SetValue(1, 2, "瞬时流量");
                        sheet.SetValue(1, 3, "瞬时流量单位");
                        sheet.SetValue(1, 4, "累积流量");
                        sheet.SetValue(1, 5, "累积流量单位");

                        for (int index = 0; index < flowDatas.Count; index++)
                        {
                            int row = index + 2;
                            for (int column = 1; column <= 5; column++)
                            {
                                sheet.Cells[row, column].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }

                            sheet.Cells[row, 1].Style.Numberformat.Format = "YYYY/MM/DD HH:mm:ss";
                            sheet.SetValue(row, 1, flowDatas[index].CollectTime);

                            sheet.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";   //瞬时流量保留两位小数
                            sheet.SetValue(row, 2, flowDatas[index].CurrentFlow);
                            sheet.SetValue(row, 3, flowDatas[index].Unit);

                            sheet.Cells[row, 4].Style.Numberformat.Format = "#,###0.000"; //累积流量保留三位小数
                            sheet.SetValue(row, 4, flowDatas[index].AccuFlow);
                            sheet.SetValue(row, 5, flowDatas[index].AccuFlowUnit);
                        }

                        for (int column = 1; column <= 5; column++)
                        {   //调整列宽度为自适应
                            sheet.Column(column).AutoFit();
                        }

                        await package.SaveAsync();
                        sheet.Dispose();
                    }
                });
                _viewModel.ShowMessage("导出完成。");
            }
            catch (Exception e)
            {
                LoggerHelper.WriteLog(e.Message, e);
                _viewModel.ShowMessage("导出Excel出错：\n" + e.Message);
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
            if (!(parameter is string parameterString))
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);
            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string parameterString))
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
    }
}
