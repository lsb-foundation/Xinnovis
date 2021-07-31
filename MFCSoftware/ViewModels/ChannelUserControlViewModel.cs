﻿using CommonLib.Extensions;
using GalaSoft.MvvmLight;
using System;
using MFCSoftware.Models;
using System.Text;
using System.Windows.Media;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using CommonLib.MfcUtils;

namespace MFCSoftware.ViewModels
{
    public class ChannelUserControlViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainVm;
        private const int xValuesCount = 300;
        public ChannelUserControlViewModel(MainWindowViewModel mainVm)
        {
            _mainVm = mainVm;
            SetPlotModel();
        }

        public PlotModel SeriesPlotModel { get; set; }

        private int _address;
        public int Address
        {
            get => _address;
            set
            {
                Set(ref _address, value);
                SetCommands();
            }
        }

        public string[] DisplayUnits { get; } = new string[] { "SCCM", "SLM", "%F.S" };

        private string _displayUnit;
        public string DisplayUnit
        {
            get => _displayUnit;
            set => Set(ref _displayUnit, value);
        }

        public SolidColorBrush StatusColor { get; private set; } = new SolidColorBrush(Colors.Transparent);

        public BaseInformation BaseInfo { get; private set; }
        public FlowData Flow { get; private set; } = new FlowData();

        private uint _insertInterval = 30;
        public uint InsertInterval
        {
            get => _insertInterval;
            set
            {
                if(value > 0)
                {
                    Set(ref _insertInterval, value);
                    InsertIntervalChanged?.Invoke(value);
                }
            }
        }

        public event Action<uint> InsertIntervalChanged;

        private float _flowValue;
        public float FlowValue
        {
            get => _flowValue;
            set => Set(ref _flowValue, value);
        }

        private float _valveOpenValue;
        public float ValveOpenValue
        {
            get => _valveOpenValue;
            set => Set(ref _valveOpenValue, value);
        }

        private ControlSelector _selector = ControlSelector.FlowValue;
        public ControlSelector Selector
        {
            get => _selector;
            set => Set(ref _selector, value);
        }

        public Visibility ControlVisibility
        {
            get
            {
                string appName = _mainVm.AppName;
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
            UpdateYAxisMaxValue(info.Range);

            RaisePropertyChanged(nameof(BaseInfo));
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

            RaisePropertyChanged(nameof(Flow));
            WhenSuccess();
        }

        public void UpdateSeries()
        {
            lock (SeriesPlotModel.SyncRoot)
            {
                var s = (OxyPlot.Series.LineSeries)SeriesPlotModel.Series[0];
                double x = DateTimeAxis.ToDouble(DateTime.Now);
                double y = Flow.CurrentFlow;
                s.Points.Add(new DataPoint(x, y));

                if (s.Points.Count > xValuesCount)
                {
                    s.Points.RemoveAt(0);
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
                Minimum = 0,
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
                InterpolationAlgorithm = InterpolationAlgorithms.CatmullRomSpline
            };
            SeriesPlotModel.Series.Add(line);
            RaisePropertyChanged(nameof(SeriesPlotModel));
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

        private void SetCurrentFlowByUnit(float flow)
        {
            //UCCM和CCM与SCCM等价，因此均按照SCCM进行处理
            var meterUnit = BaseInfo?.Unit?.Unit == "SLM" ? "SLM" : "SCCM";
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
            RaisePropertyChanged(nameof(StatusColor));
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

        public void ShowMessage(string message) => _mainVm.ShowMessage(message);

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

        //private SerialCommand<byte[]> GetSerialCommandFromBytes(List<byte[]> bytesList ,SerialCommandType type, int responseLength = 7)
        //{
        //    SerialCommandBuilder builder = new SerialCommandBuilder(type).AppendAddress(Address);
        //    foreach(byte[] bytes in bytesList)
        //    {
        //        builder.AppendBytes(bytes);
        //    }
        //    builder.AppendCrc16();
        //    return builder.ToSerialCommand(responseLength);
        //}
    }

    public enum ControlSelector
    {
        FlowValue,
        ValveOpenValue
    }
}
