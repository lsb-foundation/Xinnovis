using CommonLib.Extensions;
using CommonLib.Models;
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

        public event Action<object> ControlWasRemoved; //控件被移除

        public int Address { get => viewModel.Address; }
        public SerialCommand<byte[]> ReadFlowBytes { get => viewModel.ReadFlowBytes; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get => viewModel.ReadBaseInfoBytes; }

        private void Closed(object sender, RoutedEventArgs e)
        {
            ControlWasRemoved?.Invoke(this);
        }

        public void ResolveData(byte[] data)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (!data.CheckCRC16ByDefault()) return;

                if (data.Length == 37)
                    ResolveBaseInfoData(data);
                else if (data.Length == 27)
                {
                    //暂时先按照采集流量处理
                    ResolveFlowData(data);
                }
            });
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
        }

        private byte[] GetChildArray(byte[] array, int index, int length)
        {
            byte[] ret = new byte[length];
            Array.Copy(array, index, ret, 0, length);
            return ret;
        }

        public void WhenTimeOut()
        {
            Console.Write("接收超时。");
        }

        public void SetAddress(int addr) => viewModel.SetAddress(addr);
    }
}
