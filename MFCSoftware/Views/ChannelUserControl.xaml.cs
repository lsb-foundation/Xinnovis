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
using System.Timers;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Reflection;

namespace MFCSoftware.Views
{
    /// <summary>
    /// ChannelUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelUserControl : UserControl
    {
        private readonly ChannelUserControlViewModel viewModel = new ChannelUserControlViewModel();
        private readonly Timer timer;
        private bool canInsert = true;

        public ChannelUserControl()
        {
            InitializeComponent();
            timer = new Timer { AutoReset = false, Interval = 60 * 1000 };
            timer.Elapsed += (s, e) => canInsert = true;
            viewModel.InsertIntervalChanged += () =>
            {
                if (viewModel.InsertInterval > 0)
                {
                    timer.Interval = viewModel.InsertInterval * 60 * 1000;
                }
            };
            this.DataContext = viewModel;
            timer.Start();
        }

        public event Action<ChannelUserControl> ControlWasRemoved; //控件被移除
        public event Action<ChannelUserControl, SerialCommand<byte[]>, ResolveType> SingleCommandSended;    //单指令发送

        public int Address { get => viewModel.Address; }
        public SerialCommand<byte[]> ReadFlowBytes { get => viewModel.ReadFlowBytes; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get => viewModel.ReadBaseInfoBytes; }

        private void Closed(object sender, RoutedEventArgs e)
        {
            ControlWasRemoved?.Invoke(this);
        }

        public void ResolveData(byte[] data, ResolveType type)
        {
            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (!data.CheckCRC16ByDefault())
                        throw new Exception("CRC校验失败。");

                    switch (type)
                    {
                        case ResolveType.BaseInfoData:
                            ResolveBaseInfoData(data);
                            break;
                        case ResolveType.ReadFlow:
                            ResolveFlowData(data);
                            break;
                        case ResolveType.ClearAccuFlowData:
                        case ResolveType.SetFlow:
                        case ResolveType.ValveControl:
                        case ResolveType.ZeroPointCalibration:
                        case ResolveType.FactoryRecovery:
                            AutoResolveData(data, type);
                            break;
                    }
                }
                catch(Exception e)
                {
                    viewModel.WhenResolveFailed();
                    MainWindowViewModel.ShowAppMessage(e.Message);
                    LogHelper.WriteLog(e.Message, e);
                }
            });
        }

        private void AutoResolveData(byte[] data, ResolveType type)
        {
            var resolveNameAttr = type.GetType().GetField(type.ToString()).GetCustomAttribute<ResolveActionAttribute>();
            if (resolveNameAttr == null) return;
            if (!resolveNameAttr.AutoResolve) return;
            string message = resolveNameAttr.ActionName + (resolveNameAttr.Check(data) ? "成功。" : "失败。");
            MainWindowViewModel.ShowAppMessage(message);
        }

        private void ResolveBaseInfoData(byte[] data)
        {
            byte[] gasTypeBytes = data.SubArray(3, 2);
            int gas = gasTypeBytes.ToInt32(0, 2);

            byte[] rangeBytes = data.SubArray(5, 2);
            int range = rangeBytes.ToInt32(0, 2);

            byte[] unitBytes = data.SubArray(7, 2);
            int unit = unitBytes.ToInt32(0, 2);

            byte[] snBytes = data.SubArray(23, 12);
            string sn = snBytes.ToHexString();

            BaseInformation info = new BaseInformation()
            {
                SN = sn,
                Range = range,
                GasType = GasTypeCode.GetGasTypeCodes().FirstOrDefault(c => c.Code == gas),
                Unit = UnitCode.GetUnitCodes().FirstOrDefault(u => u.Code == unit)
            };
            viewModel.SetBaseInfomation(info);
        }

        private void ResolveFlowData(byte[] data)
        {
            //addr 0x03 0x16 FLOW1 FLOW2 FLOW3 FLOW4
            //ACCMULATE1 ACCMULATE2 ACCMULATE3 ACCMULATE4 ACCMULATE5 ACCMULATE6 ACCMULATE7 ACCMULATE8
            //UNIT1 UNIT2 DAY1 DAY2 HOUR1 HOUR2 MIN1 MIN2 SEC1 SEC2
            //CRCL CRCH
            byte[] flowBytes = data.SubArray(3, 4);
            float flow = flowBytes.ToInt32(0, 4) / 100.0f;

            byte[] accuFlowBytes = data.SubArray(7, 8);
            float accuFlow = BitConverter.ToInt64(accuFlowBytes.Reverse().ToArray(),0) / 1000.0f;

            byte[] unitBytes = data.SubArray(15, 2);
            int unitCode = unitBytes.ToInt32(0, 2);
            string unit = string.Empty;
            if (unitCode == 0) unit = "L";
            else if (unitCode == 1) unit = "m³";

            byte[] daysBytes = data.SubArray(17, 2);
            int days = daysBytes.ToInt32(0, 2);

            byte[] hoursBytes = data.SubArray(19, 2);
            int hours = hoursBytes.ToInt32(0, 2);

            byte[] minsBytes = data.SubArray(21, 2);
            int mins = minsBytes.ToInt32(0, 2);

            byte[] secsBytes = data.SubArray(23, 2);
            int secs = secsBytes.ToInt32(0, 2);

            FlowData flowData = new FlowData()
            {
                CurrentFlow = flow,
                Unit = viewModel.BaseInfo.Unit?.Unit,
                AccuFlow = accuFlow,
                AccuFlowUnit = unit,
                Days = days,
                Hours = hours,
                Minutes = mins,
                Seconds = secs
            };
            viewModel.SetFlow(flowData);
            viewModel.UpdateSeries();

            if (canInsert)
            {
                DbStorage.InsertFlowData(Address, flowData);
                canInsert = false;
                timer.Start();
            }
        }

        public void WhenTimeOut()
        {
            this.Dispatcher.Invoke(() =>
            {
                viewModel.WhenTimeOut();
#if DEBUG
                Console.WriteLine($"Channel {Address}: TimeoutException");
#endif
            });
        }

        public void SetAddress(int addr) => viewModel.SetAddress(addr);

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
                    MainWindowViewModel.ShowAppMessage("未查询到数据！");
                }
            }
        }

        public async void ExportHistoryFlowDataToExcel(string fileName, List<FlowData> flowDatas)
        {
            try
            {
                await Task.Run(async () =>
                {
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
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
                MainWindowViewModel.ShowAppMessage("导出完成。");
            }
            catch (Exception e)
            {
                LogHelper.WriteLog(e.Message, e);
                MainWindowViewModel.ShowAppMessage("导出Excel出错：\n" + e.Message);
            }
        }

        private void ClearAccuFlowButton_Click(object sender, RoutedEventArgs e)
        {
            var mbxResult = MessageBox.Show("是否确认清除？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbxResult == MessageBoxResult.Yes)
            {
                SingleCommandSended?.Invoke(this, viewModel.ClearAccuFlowBytes, ResolveType.ClearAccuFlowData);
            }
        }

        private void ControlButton_Clicked(object sender, RoutedEventArgs e)
        {
            if(viewModel.Selector == ControlSelector.FlowValue)
            {
                CheckFlowValueAndSendCommand();
            }
            else if(viewModel.Selector == ControlSelector.ValveOpenValue)
            {
                CheckValveOpenValueAndSendCommand();
            }
        }

        private void CheckFlowValueAndSendCommand()
        {
            if (viewModel.BaseInfo == null)
            {
                MainWindowViewModel.ShowAppMessage("未获取到基础数据，量程未知。");
                return;
            }
            if (viewModel.FlowValue < 0 || viewModel.FlowValue > viewModel.BaseInfo.Range)
            {
                MainWindowViewModel.ShowAppMessage("流量数据必须大于等于0，小于等于量程。");
                return;
            }
            viewModel.SetWriteFlowBytes();
            SingleCommandSended?.Invoke(this, viewModel.WriteFlowBytes, ResolveType.SetFlow);
        }

        private void CheckValveOpenValueAndSendCommand()
        {
            if(viewModel.ValveOpenValue < 0 || viewModel.ValveOpenValue > 100)
            {
                MainWindowViewModel.ShowAppMessage("阀门开度必须大于等于0，小于等于100。");
                return;
            }
            viewModel.SetWriteValveBytes();
            SingleCommandSended?.Invoke(this, viewModel.WriteValveBytes, ResolveType.ValveControl);
        }

        private void SetSaveTimeButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (uint.TryParse(saveIntervalTextBox.Text, out uint _))
                MainWindowViewModel.ShowAppMessage("保存时间间隔设置成功。");
            else MainWindowViewModel.ShowAppMessage("输入有误，请重新输入。");
        }

        private void ZeroPointCaliButton_Click(object sender, RoutedEventArgs e)
        {
            var mbxResult = MessageBox.Show("零点校准确认？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbxResult == MessageBoxResult.Yes)
            {
                SingleCommandSended?.Invoke(this, viewModel.ZeroPointCalibrationBytes, ResolveType.ZeroPointCalibration);
            }
        }

        private void FactoryRecoveryButton_Click(object sender, RoutedEventArgs e)
        {
            var mbxResult = MessageBox.Show("恢复出厂确认？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbxResult == MessageBoxResult.Yes)
            {
                SingleCommandSended?.Invoke(this, viewModel.FactoryRecoveryBytes, ResolveType.FactoryRecovery);
            }
        }
    }

    public enum ResolveType
    {
        [ResolveAction("获取基本信息")]
        BaseInfoData,

        [ResolveAction("读取流量")]
        ReadFlow,

        [ResolveAction("清除累积流量", true, new byte[] { 0x06, 0x02, 0x00, 0x00 })]
        ClearAccuFlowData,

        [ResolveAction("设置流量", true, new byte[] { 0x06, 0x02, 0x00, 0x00 })]
        SetFlow,

        [ResolveAction("设置阀门开度", true, new byte[] { 0x06, 0x02, 0x00, 0x03 })]
        ValveControl,

        [ResolveAction("零点校准", true, new byte[] { 0x06, 0x02, 0x00, 0x01 })]
        ZeroPointCalibration,

        [ResolveAction("恢复出厂", true, new byte[] { 0x06, 0x02, 0x00, 0x02 })]
        FactoryRecovery
    }

    /// <summary>
    /// 用于实现自动解析的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ResolveActionAttribute : Attribute
    {
        public string ActionName { get; }
        public bool AutoResolve { get; }
        public byte[] CheckBytes { get; }
        public ResolveActionAttribute(string actionName, bool autoResolve = false, byte[] checkBytes = null)
        {
            ActionName = actionName;
            AutoResolve = AutoResolve;
            CheckBytes = checkBytes;
        }

        public bool Check(byte[] data)
        {
            if (!AutoResolve) return true;
            if (CheckBytes == null || CheckBytes.Length <= 0) return false;
            if (CheckBytes.Length != data.Length - 3) return false;     //不校验地址及CRC

            for(int index = 0; index < CheckBytes.Length; index++)
            {
                if (CheckBytes[index] != data[index + 1])
                    return false;
            }
            return true;
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
