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
        public event Action<ChannelUserControl> ClearAccuFlowClicked; //清除累积流量

        public int Address { get => viewModel.Address; }
        public SerialCommand<byte[]> ReadFlowBytes { get => viewModel.ReadFlowBytes; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get => viewModel.ReadBaseInfoBytes; }
        public SerialCommand<byte[]> ClearAccuFlowBytes { get => viewModel.ClearAccuFlowBytes; }

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

                    if (type == ResolveType.BaseInfoData)
                        ResolveBaseInfoData(data);
                    else if (type == ResolveType.FlowData)
                        ResolveFlowData(data);
                    else if (type == ResolveType.ClearAccuFlowData)
                        ResolveClearAccuFlowData(data);
                }
                catch(Exception e)
                {
                    viewModel.WhenResolveFailed();
                    LogHelper.WriteLog(e.Message, e);
                }
            });
        }

        private void ResolveClearAccuFlowData(byte[] data)
        {
            //addr 0x06 0x02 0x00 0x00 CRCL CRCH
            bool success = data[1] == 0x06 && data[2] == 0x02 && data[3] == 0x00 && data[4] == 0x00;
            if (success) viewModel.WhenSuccess();
            else throw new Exception("累积流量清除失败。");
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
                Console.WriteLine($"Channel {Address}: TimeoutException");
            });
        }

        public void SetAddress(int addr) => viewModel.SetAddress(addr);

        //递归查找当前控件的根节点Window对象
        private DependencyObject GetCurrentWindow(DependencyObject obj)
        {
            if (obj is Window) return obj;
            return GetCurrentWindow(VisualTreeHelper.GetParent(obj));
        }

        private void ExportFlowButton_Click(object sender, RoutedEventArgs e)
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
                    DbStorage.QueryFlowDatasByTime(win.FromTime, win.ToTime, Address) :
                    DbStorage.QueryAllFlowDatas(Address);

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
                    MessageBox.Show("未查询到数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public async void ExportHistoryFlowDataToExcel(string fileName, List<FlowData> flowDatas)
        {
            try
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
                    for(int column = 1; column <=5; column++)
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

                    for(int column = 1; column <= 5; column++)
                    {   //调整列宽度为自适应
                        sheet.Column(column).AutoFit();
                    }

                    await package.SaveAsync();
                    sheet.Dispose();
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog(e.Message, e);
                MessageBox.Show("导出Excel出错：\n" + e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAccuFlowButton_Click(object sender, RoutedEventArgs e)
        {
            var mbxResult = MessageBox.Show("是否确认清除？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (mbxResult == MessageBoxResult.Yes)
            {
                ClearAccuFlowClicked?.Invoke(this);
            }
        }
    }

    public enum ResolveType
    {
        BaseInfoData,
        FlowData,
        ClearAccuFlowData
    }

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
}
