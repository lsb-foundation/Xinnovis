using CommonLib.Extensions;
using CommonLib.Models;
using MFCSoftware.Common;
using MFCSoftware.Models;
using MFCSoftware.ViewModels;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis.TokenSeparatorHandlers;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using OfficeOpenXml;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;

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
                if (!data.CheckCRC16ByDefault()) return;

                if (type == ResolveType.BaseInfoData)
                    ResolveBaseInfoData(data);
                else if (type == ResolveType.FlowData)
                    ResolveFlowData(data);
                else if (type == ResolveType.ClearAccuFlowData)
                    ResolveClearAccuFlowData(data);
            });
        }

        private void ResolveClearAccuFlowData(byte[] data)
        {
            //addr 0x06 0x02 0x00 0x00 CRCL CRCH

        }

        private void ResolveBaseInfoData(byte[] data)
        {
            byte[] gasTypeBytes = GetChildArray(data, 3, 2);
            int gas = gasTypeBytes.ToInt32(0, 2);

            byte[] rangeBytes = GetChildArray(data, 5, 2);
            int range = rangeBytes.ToInt32(0, 2);

            byte[] unitBytes = GetChildArray(data, 7, 2);
            int unit = unitBytes.ToInt32(0, 2);

            byte[] snBytes = GetChildArray(data, 23, 12);
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
            byte[] flowBytes = GetChildArray(data, 3, 4);
            float flow = flowBytes.ToInt32(0, 4) / 100.0f;

            byte[] accuFlowBytes = GetChildArray(data, 7, 8);
            float accuFlow = BitConverter.ToInt64(accuFlowBytes.Reverse().ToArray(),0) / 1000.0f;

            byte[] unitBytes = GetChildArray(data, 15, 2);
            int unitCode = unitBytes.ToInt32(0, 2);
            string unit = string.Empty;
            if (unitCode == 0) unit = "SCCM";
            if (unitCode == 1) unit = "SLM";

            byte[] daysBytes = GetChildArray(data, 17, 2);
            int days = daysBytes.ToInt32(0, 2);

            byte[] hoursBytes = GetChildArray(data, 19, 2);
            int hours = hoursBytes.ToInt32(0, 2);

            byte[] minsBytes = GetChildArray(data, 21, 2);
            int mins = minsBytes.ToInt32(0, 2);

            byte[] secsBytes = GetChildArray(data, 23, 2);
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

        private byte[] GetChildArray(byte[] array, int index, int length)
        {
            byte[] ret = new byte[length];
            Array.Copy(array, index, ret, 0, length);
            return ret;
        }

        public void WhenTimeOut()
        {
            
        }

        public void SetAddress(int addr) => viewModel.SetAddress(addr);

        private void ExportFlowButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Excel文件|*.xlsx;*.xls";
            dialog.Title = "保存Excel文件";
            if ((bool)dialog.ShowDialog())
            {
                if (!string.IsNullOrEmpty(dialog.FileName))
                {
                    ExportHistoryToExcel(dialog.FileName);
                }
            }
        }

        public async void ExportHistoryToExcel(string fileName)
        {
            try
            {
                var flowDatas = DbStorage.QueryLastest2HoursFlowData(Address);
                if (flowDatas.Count > 0)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet sheet;
                        if (package.Workbook.Worksheets.Any(e => e.Name == "流量数据表"))
                            sheet = package.Workbook.Worksheets.FirstOrDefault(e => e.Name == "流量数据表");
                        else sheet = package.Workbook.Worksheets.Add("流量数据表");

                        //添加表头
                        sheet.SetValue(0, 0, "瞬时流量");
                        sheet.SetValue(0, 1, "累积流量");
                        sheet.SetValue(0, 2, "采样时间");
                        sheet.SetValue(0, 3, "单位");

                        for (int index = 0; index < flowDatas.Count; index++)
                        {
                            sheet.SetValue(index + 1, 0, flowDatas[index].CurrentFlow);
                            sheet.SetValue(index + 1, 1, flowDatas[index].AccuFlow);
                            sheet.SetValue(index + 1, 2, flowDatas[index].AccuFlow);
                            sheet.SetValue(index + 1, 3, flowDatas[index].Unit);
                        }
                        await package.SaveAsync();
                        sheet.Dispose();
                    }
                }
                else
                {
                    MessageBox.Show("未查询到数据！");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("导出Excel出错：\n" + e.Message);
            }
        }

        private void ClearAccuFlowButton_Click(object sender, RoutedEventArgs e)
        {
            ClearAccuFlowClicked?.Invoke(this);
        }
    }

    public enum ResolveType
    {
        BaseInfoData,
        FlowData,
        ClearAccuFlowData
    }
}
