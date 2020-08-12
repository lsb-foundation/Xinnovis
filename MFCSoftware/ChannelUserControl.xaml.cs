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
using Microsoft.Win32;
using System.IO;
using System.Globalization;

namespace MFCSoftware
{
    /// <summary>
    /// ChannelUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ChannelUserControl : UserControl
    {
        private ChannelUserControlViewModel viewModel = new ChannelUserControlViewModel();
        
        public ChannelUserControl()
        {
            InitializeComponent();
            this.DataContext = viewModel;
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
                    if (!data.CheckCRC16ByDefault()) return;

                    if (type == ResolveType.BaseInfoData)
                        ResolveBaseInfoData(data);
                    else if (type == ResolveType.FlowData)
                        ResolveFlowData(data);
                    else if (type == ResolveType.ClearAccuFlowData)
                        ResolveClearAccuFlowData(data);
                }
                catch(Exception e)
                {
                    LogHelper.WriteLog(e.Message, e);
                }
            });
        }

        private void ResolveClearAccuFlowData(byte[] data)
        {
            //addr 0x06 0x02 0x00 0x00 CRCL CRCH
            
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
            if (unitCode == 0) unit = "SCCM";
            if (unitCode == 1) unit = "SLM";

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
                AccuFlow = accuFlow,
                Unit = unit,
                Days = days,
                Hours = hours,
                Minutes = mins,
                Seconds = secs
            };
            viewModel.SetFlow(flowData);
            viewModel.UpdateSeries();
            DbStorage.InsertFlowData(Address, flowData);
        }

        public void WhenTimeOut()
        {
            
        }

        public void SetAddress(int addr) => viewModel.SetAddress(addr);

        private void ExportFlowButton_Click(object sender, RoutedEventArgs e)
        {
            var flowDatas = DbStorage.QueryLastest2HoursFlowData(Address);
            if (flowDatas.Count > 0)
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Excel文件|*.xlsx;*.xls";
                dialog.Title = "保存Excel文件";
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
                MessageBox.Show("未查询到数据！");
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

                    sheet.SetValue(0, 0, "瞬时流量");
                    sheet.SetValue(0, 1, "累积流量");
                    sheet.SetValue(0, 2, "采样时间");
                    sheet.SetValue(0, 3, "单位");

                    for (int index = 0; index < flowDatas.Count; index++)
                    {
                        sheet.SetValue(index + 1, 0, flowDatas[index].CurrentFlow);
                        sheet.SetValue(index + 1, 1, flowDatas[index].AccuFlow);
                        sheet.SetValue(index + 1, 2, flowDatas[index].CollectTime);
                        sheet.SetValue(index + 1, 3, flowDatas[index].Unit);
                    }
                    await package.SaveAsync();
                    sheet.Dispose();
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog(e.Message, e);
                MessageBox.Show("导出Excel出错：\n" + e.Message);
            }
        }

        private void ClearAccuFlowButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定清除？", "清除确认", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                ClearAccuFlowClicked?.Invoke(this);
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
            var flowData = value as FlowData;
            var timeText = string.Empty;
            if (flowData != null)
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
