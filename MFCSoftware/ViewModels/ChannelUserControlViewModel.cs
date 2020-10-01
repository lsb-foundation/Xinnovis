using CommonLib.Extensions;
using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using MFCSoftware.Models;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;
using System.Text;
using System.Windows.Media;
using System.Linq;
using System.Windows;

namespace MFCSoftware.ViewModels
{
    public class ChannelUserControlViewModel:BindableBase
    {
        private const int xValuesCount = 300;
        public ChannelUserControlViewModel()
        {
            InitializeFlowSeries();
        }

        public SeriesCollection FlowSeries { get; set; }
        public Func<double, string> FlowLabelsFomatter
        {
            get => val => string.Format("{0:N2}", val);
        }

        private double _maxYValue = 100;
        public double MaxYValue
        {   //Y轴最大值
            get => _maxYValue;
            set
            {
                if (value <= 0) return;
                SetProperty(ref _maxYValue, value);
            }
        }

        public int Address { get; private set; }

        public string[] DisplayUnits { get; } = new string[] { "SCCM", "SLM", "%F.S" };

        private string _displayUnit;
        public string DisplayUnit
        {
            get => _displayUnit;
            set => SetProperty(ref _displayUnit, value);
        }

        public SolidColorBrush StatusColor { get; private set; } = new SolidColorBrush(Colors.Transparent);

        public BaseInformation BaseInfo { get; private set; }
        public FlowData Flow { get; private set; } = new FlowData();

        private uint _insertInterval = 1;
        public uint InsertInterval
        {
            get => _insertInterval;
            set
            {
                if(value > 0)
                {
                    SetProperty(ref _insertInterval, value);
                    InsertIntervalChanged?.Invoke(value);
                }
            }
        }

        public event Action<uint> InsertIntervalChanged;

        private float _flowValue;
        public float FlowValue
        {
            get => _flowValue;
            set => SetProperty(ref _flowValue, value);
        }

        private float _valveOpenValue;
        public float ValveOpenValue
        {
            get => _valveOpenValue;
            set => SetProperty(ref _valveOpenValue, value);
        }

        private ControlSelector _selector = ControlSelector.FlowValue;
        public ControlSelector Selector
        {
            get => _selector;
            set => SetProperty(ref _selector, value);
        }

        public Visibility ControlVisibility
        {
            get
            {
                string appName = ViewModelBase.GetViewModelInstance<MainWindowViewModel>().AppName;
                return appName.Contains("MFC") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public SerialCommand<byte[]> ReadFlowBytes { get; private set; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get; private set; }
        public SerialCommand<byte[]> ClearAccuFlowBytes { get; private set; }
        public SerialCommand<byte[]> WriteFlowBytes { get; private set; }
        public SerialCommand<byte[]> WriteValveBytes { get; private set; }
        public SerialCommand<byte[]> ZeroPointCalibrationBytes { get; private set; }
        public SerialCommand<byte[]> FactoryRecoveryBytes { get; private set; }

        public void SetAddress(int addr)
        {
            Address = addr;
            RaiseProperty(nameof(Address));
            SetCommands();
        }

        private void SetCommands()
        {
            SetReadFlowBytes();
            SetReadBaseInfoBytes();
            SetClearAccuFlowBytes();
            SetZeroPointCalibrationBytes();
            SetFactoryRecoveryBytes();
        }

        public void SetBaseInfomation(BaseInformation info)
        {
            if (BaseInfo == null)
                BaseInfo = new BaseInformation();

            BaseInfo.SN = ConvertSN(info.SN);
            BaseInfo.Range = info.Range;
            BaseInfo.GasType = info.GasType;
            BaseInfo.Unit = info.Unit;
            DisplayUnit = info.Unit?.Unit;
            MaxYValue = info.Range; //更新Y轴最大值

            RaiseProperty(nameof(BaseInfo));
            WhenSuccess();
        }

        private string ConvertSN(string sn)
        {
            var builder = new StringBuilder();
            string[] splitter = sn.Split(' ');
            for(int index = 0; index < splitter.Length; index++)
            {
                builder.Append(splitter[index]);
                if ((index + 1) % 2 == 0 && index < splitter.Length - 1)
                    builder.Append(" ");
            }
            return builder.ToString();
        }

        public void SetFlow(FlowData flow)
        {
            if (Flow == null) 
                Flow = new FlowData();

            SetCurrentFlowByUnit(flow.CurrentFlow);
            Flow.AccuFlow = flow.AccuFlow;
            Flow.AccuFlowUnit = flow.AccuFlowUnit;
            Flow.Days = flow.Days;
            Flow.Hours = flow.Hours;
            Flow.Minutes = flow.Minutes;
            Flow.Seconds = flow.Seconds;

            RaiseProperty(nameof(Flow));
            WhenSuccess();
        }

        public void UpdateSeries()
        {
            FlowSeries[0].Values.Add(new ObservableValue(Flow.CurrentFlow));
            FlowSeries[0].Values.RemoveAt(0);
        }

        private void InitializeFlowSeries()
        {
            var values = new ChartValues<ObservableValue>();
            for(int index = 0; index < xValuesCount; index++)
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
            //UCCM和CCM与SCCM等价，因此均按照SCCM进行处理
            var meterUnit = BaseInfo.Unit?.Unit == "SLM" ? "SLM" : "SCCM";
            Func<float, float> converterFunc = v => v;       //默认不转换

            if (meterUnit == "SCCM")
            {
                if (DisplayUnit == "SLM")           //SCCM->SLM
                    converterFunc = v => v / 1000;
                else if (DisplayUnit == "%F.S")     //SCCM->%F.S
                    converterFunc = v => v / BaseInfo.Range * 100;
            }
            else
            {
                if (DisplayUnit == "SCCM")          //SLM->SCCM
                    converterFunc = v => v * 1000;
                else if (DisplayUnit == "%F.S")     //SLM->%F.S
                    converterFunc = v => v / BaseInfo.Range * 100;
            }

            Flow.CurrentFlow = converterFunc.Invoke(flow);
        }

        public void WhenTimeOut() => SetStatusColor(ReceivedStatus.Timeout);
        public void WhenSuccess() => SetStatusColor(ReceivedStatus.Success);
        public void WhenResolveFailed() => SetStatusColor(ReceivedStatus.ResolveFailed);

        private void SetStatusColor(ReceivedStatus status)
        {
            if (status == ReceivedStatus.Success)
                StatusColor.Color = Colors.Green;
            else if (status == ReceivedStatus.ResolveFailed)
                StatusColor.Color = Colors.Yellow;
            else if (status == ReceivedStatus.Timeout)
                StatusColor.Color = Colors.Red;
            RaiseProperty(nameof(StatusColor));
        }

        private void SetReadFlowBytes()
        {
            //addr 0x03 0x00 0x16 0x00 0x0B CRCL CRCH
            var bytes = new byte[] { 0x03, 0x00, 0x16, 0x00, 0x0B };
            ReadFlowBytes = GetSerialCommandFromBytes(bytes, 27);
        }

        private void SetReadBaseInfoBytes()
        {
            //addr 0x03 0x00 0x03 0x00 0x10 CRCL CRCH
            var bytes = new byte[] { 0x03, 0x00, 0x03, 0x00, 0x10 };
            ReadBaseInfoBytes = GetSerialCommandFromBytes(bytes, 37);
        }

        private void SetClearAccuFlowBytes()
        {
            //addr 0x06 0x00 0x18 0x00 0x00 CRCL CRCH
            var bytes = new byte[] { 0x06, 0x00, 0x18, 0x00, 0x00 };
            ClearAccuFlowBytes = GetSerialCommandFromBytes(bytes, 7);
        }

        private void SetZeroPointCalibrationBytes()
        {
            //addr 0x06 0x00 0x25 0x00 0x01 CRCL CRCH
            var bytes = new byte[] { 0x06, 0x00, 0x25, 0x00, 0x01 };
            ZeroPointCalibrationBytes = GetSerialCommandFromBytes(bytes, 7);
        }

        private void SetFactoryRecoveryBytes()
        {
            //addr 0x06 0x00 0x25 0x00 0x02 CRCL CRCH
            var bytes = new byte[] { 0x06, 0x00, 0x25, 0x00, 0x02 };
            FactoryRecoveryBytes = GetSerialCommandFromBytes(bytes, 7);
        }

        public void SetWriteFlowBytes()
        {
            //addr 0x06 0x00 0x21 0x00 0x00 [FLOW_1] {FLOW_2] {FLOW_3] {FLOW_4] CRCL CRCH
            int flowIntValue = ParseFloatToInt32(FlowValue * 100);
            WriteFlowBytes = new SerialCommandBuilder()
                .AppendAddress(Address)
                .AppendBytes(new byte[] { 0x06, 0x00, 0x21, 0x00, 0x00 })
                .AppendBytes(flowIntValue.ToHex())
                .AppendCrc16()
                .ToSerialCommand(7);
        }

        public void SetWriteValveBytes()
        {
            //addr 0x06 0x00 0x21 0x00 0x03 [VALVE_VALUE_1] [VALVE_VALUE_2] CRCL CRCH
            int openIntValue = ParseFloatToInt32(ValveOpenValue * 100);
            WriteValveBytes = new SerialCommandBuilder()
                .AppendAddress(Address)
                .AppendBytes(new byte[] { 0x06, 0x00, 0x21, 0x00, 0x03 })
                .AppendBytes(openIntValue.ToHex().SubArray(2, 2))
                .AppendCrc16()
                .ToSerialCommand(7);
        }

        private int ParseFloatToInt32(float value)
        {
            //防止float直接转int导致精度丢失
            var valueStr = value.ToString("#0.000").Split('.')[0];
            return int.Parse(valueStr);
        }

        private SerialCommand<byte[]> GetSerialCommandFromBytes(byte[] bytes, int returnedLength)
        {
            return new SerialCommandBuilder()
                .AppendAddress(Address)
                .AppendBytes(bytes)
                .AppendCrc16()
                .ToSerialCommand(returnedLength);
        }

        private SerialCommand<byte[]> GetSerialCommandFromBytes(List<byte[]> bytesList, int returnedLength)
        {
            SerialCommandBuilder builder = new SerialCommandBuilder().AppendAddress(Address);
            foreach(byte[] bytes in bytesList)
            {
                builder.AppendBytes(bytes);
            }
            builder.AppendCrc16();
            return builder.ToSerialCommand(returnedLength);
        }

        enum ReceivedStatus
        {
            Success,
            ResolveFailed,
            Timeout
        }
    }

    public enum ControlSelector
    {
        FlowValue,
        ValveOpenValue
    }
}
