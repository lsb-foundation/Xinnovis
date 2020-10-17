using CommonLib.Extensions;
using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using MFCSoftware.Models;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Geared;
using LiveCharts.Defaults;
using System.Text;
using System.Windows.Media;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;

namespace MFCSoftware.ViewModels
{
    public class ChannelUserControlViewModel:BindableBase
    {
        private const int xValuesCount = 300;
        public ChannelUserControlViewModel()
        {
            //InitializeFlowSeries();
            SetPlotModel();
        }

        public PlotModel SeriesPlotModel { get; set; }
        //public SeriesCollection FlowSeries { get; set; }
        //public Func<double, string> FlowLabelsFomatter
        //{
        //    get => val => string.Format("{0:N2}", val);
        //}

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
            byte[] bytes = new byte[] { 0x03, 0x00, 0x16, 0x00, 0x0B };
            ReadFlowBytes = GetSerialCommandFromBytes(bytes, SerialCommandType.ReadFlow, 27);
            bytes = new byte[] { 0x03, 0x00, 0x03, 0x00, 0x10 };
            ReadBaseInfoBytes = GetSerialCommandFromBytes(bytes, SerialCommandType.BaseInfoData, 37);
            bytes = new byte[] { 0x06, 0x00, 0x18, 0x00, 0x00 };
            ClearAccuFlowBytes = GetSerialCommandFromBytes(bytes, SerialCommandType.ClearAccuFlowData);
            bytes = new byte[] { 0x06, 0x00, 0x25, 0x00, 0x01 };
            ZeroPointCalibrationBytes = GetSerialCommandFromBytes(bytes, SerialCommandType.ZeroPointCalibration);
            bytes = new byte[] { 0x06, 0x00, 0x25, 0x00, 0x02 };
            FactoryRecoveryBytes = GetSerialCommandFromBytes(bytes, SerialCommandType.FactoryRecovery);
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
            //MaxYValue = info.Range; //更新Y轴最大值
            UpdateYAxisMaxValue(info.Range);

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
            //FlowSeries[0].Values.Add(new ObservableValue(Flow.CurrentFlow));
            //FlowSeries[0].Values.RemoveAt(0);

            lock (SeriesPlotModel.SyncRoot)
            {
                for (int i = 0; i < SeriesPlotModel.Series.Count; i++)
                {
                    var s = (OxyPlot.Series.LineSeries)SeriesPlotModel.Series[i];

                    double x = DateTimeAxis.ToDouble(DateTime.Now);
                    double y = Flow.CurrentFlow;
                    s.Points.Add(new DataPoint(x, y));

                    if (s.Points.Count > xValuesCount)
                    {
                        s.Points.RemoveAt(0);
                    }
                }
            }
            SeriesPlotModel.InvalidatePlot(true);
        }

        private void SetPlotModel()
        {
            SeriesPlotModel = new PlotModel();

            var dateTimeAxis = new DateTimeAxis
            {
                Title = "时间",
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                IntervalType = DateTimeIntervalType.Seconds,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                IntervalLength = 50,
                TitleFontSize = 16
            };
            SeriesPlotModel.Axes.Add(dateTimeAxis);

            var valueAxis = new LinearAxis
            {
                Title = "瞬时流量",
                LabelFormatter = num => $"{num:N2}",
                IsZoomEnabled = false,
                IsPanEnabled = false,
                Position = OxyPlot.Axes.AxisPosition.Left,
                TitleFontSize = 16
            };
            SeriesPlotModel.Axes.Add(valueAxis);

            var line = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerStrokeThickness = 2.5,
                InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline
            };
            SeriesPlotModel.Series.Add(line);
            RaiseProperty(nameof(SeriesPlotModel));
        }

        public void UpdateYAxisMaxValue(double yMax)
        {
            foreach (var axis in SeriesPlotModel.Axes)
            {
                if (axis.GetType() == typeof(LinearAxis))
                {
                    axis.Maximum = yMax;
                    //RaiseProperty(nameof(SeriesPlotModel));
                }
            }
        }

        //private void InitializeFlowSeries()
        //{
        //    var values = new GearedValues<ObservableValue>();
        //    values.WithQuality(Quality.Highest);
        //    for(int index = 0; index < xValuesCount; index++)
        //    {
        //        var value = new ObservableValue();
        //        values.Add(value);
        //    }
        //    FlowSeries = new SeriesCollection()
        //    {
        //        new GLineSeries()
        //        {
        //            AreaLimit = -10,
        //            Values = values
        //        }
        //    };
        //}

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

        public void SetWriteFlowBytes()
        {
            //addr 0x06 0x00 0x21 0x00 0x00 [FLOW_1] {FLOW_2] {FLOW_3] {FLOW_4] CRCL CRCH
            int flowIntValue = ParseFloatToInt32(FlowValue * 100);
            WriteFlowBytes = new SerialCommandBuilder(SerialCommandType.SetFlow)
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
            WriteValveBytes = new SerialCommandBuilder(SerialCommandType.ValveControl)
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

        private SerialCommand<byte[]> GetSerialCommandFromBytes(byte[] bytes, SerialCommandType type, int responseLength = 7)
        {
            return new SerialCommandBuilder(type)
                .AppendAddress(Address)
                .AppendBytes(bytes)
                .AppendCrc16()
                .ToSerialCommand(responseLength);
        }

        private SerialCommand<byte[]> GetSerialCommandFromBytes(List<byte[]> bytesList ,SerialCommandType type, int responseLength = 7)
        {
            SerialCommandBuilder builder = new SerialCommandBuilder(type).AppendAddress(Address);
            foreach(byte[] bytes in bytesList)
            {
                builder.AppendBytes(bytes);
            }
            builder.AppendCrc16();
            return builder.ToSerialCommand(responseLength);
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
