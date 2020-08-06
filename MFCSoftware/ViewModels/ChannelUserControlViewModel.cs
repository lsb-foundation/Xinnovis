using CommonLib.Extensions;
using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MFCSoftware.Models;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;

namespace MFCSoftware.ViewModels
{
    public class ChannelUserControlViewModel:BindableBase
    {
        public ChannelUserControlViewModel()
        {
            InitializeFlowSeries();
        }

        public SeriesCollection FlowSeries { get; set; }
        public Func<double, string> FlowLabelsFomatter
        {
            get => val => string.Format("{0:N2}", val);
        }

        public int Address { get; private set; }

        private string _displayUnit;
        public string DisplayUnit
        {
            get => _displayUnit;
            set => SetProperty(ref _displayUnit, value);
        }

        public BaseInformation BaseInfo { get; private set; }
        public FlowData Flow { get; private set; } = new FlowData();

        public SerialCommand<byte[]> ReadFlowBytes { get; private set; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get; private set; }
        public SerialCommand<byte[]> ClearAccuFlowBytes { get; private set; }

        public void SetAddress(int addr)
        {
            Address = addr;
            RaiseProperty(nameof(Address));
            SetReadFlowBytes();
            SetReadBaseInfoBytes();
            SetClearAccuFlowBytes();
        }

        public void SetBaseInfomation(BaseInformation info)
        {
            if (BaseInfo == null)
                BaseInfo = new BaseInformation();

            BaseInfo.SN = info.SN;
            BaseInfo.Range = info.Range;
            BaseInfo.GasType = info.GasType;
            BaseInfo.Unit = info.Unit;
            DisplayUnit = info.Unit.Unit;
            RaiseProperty(nameof(BaseInfo));
        }

        public void SetFlow(FlowData flow)
        {
            if (Flow == null) 
                Flow = new FlowData();

            //Flow.CurrentFlow = flow.CurrentFlow;
            SetCurrentFlowByUnit(flow.CurrentFlow);
            Flow.AccuFlow = flow.AccuFlow;
            Flow.Unit = flow.Unit;
            Flow.Days = flow.Days;
            Flow.Hours = flow.Hours;
            Flow.Minutes = flow.Minutes;
            Flow.Seconds = flow.Seconds;

            RaiseProperty(nameof(Flow));
        }

        public void UpdateSeries()
        {
            FlowSeries[0].Values.Add(new ObservableValue(Flow.CurrentFlow));
            FlowSeries[0].Values.RemoveAt(0);
        }

        private void InitializeFlowSeries()
        {
            var values = new ChartValues<ObservableValue>();
            for(int index = 0; index < 100; index++)
            {
                var value = new ObservableValue();
                values.Add(value);
            }
            FlowSeries = new SeriesCollection()
            {
                new LineSeries()
                {
                    AreaLimit = -10,
                    Values = values
                }
            };
        }

        private void SetCurrentFlowByUnit(float flow)
        {
            Func<float, float> func;
            if (BaseInfo.Unit.Unit == "SCCM" && DisplayUnit == "SLM")
                func = f => f / 1000;
            else if (BaseInfo.Unit.Unit == "SCCM" && DisplayUnit == "%F.S")
                func = f => f / BaseInfo.Range * 100;
            else func = f => f;
            Flow.CurrentFlow = func.Invoke(flow);
        }

        private void SetReadFlowBytes()
        {
            //addr 0x03 0x00 0x16 0x00 0x0B CRCL CRCH
            byte addr = Convert.ToByte(Address);
            List<byte> bytes = new List<byte>();
            bytes.Add(addr);
            bytes.AddRange(new byte[] { 0x03, 0x00, 0x16, 0x00, 0x0B });
            var crc = bytes.ToArray().GetCRC16(bytes.Count);
            bytes.AddRange(crc);
            ReadFlowBytes = new SerialCommand<byte[]>(bytes.ToArray(), 27);
        }

        private void SetReadBaseInfoBytes()
        {
            //addr 0x03 0x00 0x03 0x00 0x10 CRCL CRCH
            byte addr = Convert.ToByte(Address);
            List<byte> bytes = new List<byte>();
            bytes.Add(addr);
            bytes.AddRange(new byte[] { 0x03, 0x00, 0x03, 0x00, 0x10 });
            var crc = bytes.ToArray().GetCRC16(bytes.Count);
            bytes.AddRange(crc);
            ReadBaseInfoBytes = new SerialCommand<byte[]>(bytes.ToArray(), 37);
        }

        private void SetClearAccuFlowBytes()
        {
            //addr 0x06 0x00 0x18 0x00 0x00 CRCL CRCH
            byte addr = Convert.ToByte(Address);
            List<byte> bytes = new List<byte>();
            bytes.Add(addr);
            bytes.AddRange(new byte[] { 0x06, 0x00, 0x18, 0x00, 0x00 });
            var crc = bytes.ToArray().GetCRC16(bytes.Count);
            bytes.AddRange(crc);
            ClearAccuFlowBytes = new SerialCommand<byte[]>(bytes.ToArray(), 7);
        }
    }
}
