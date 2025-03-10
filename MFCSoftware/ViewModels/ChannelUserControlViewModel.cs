﻿using CommonLib.Extensions;
using System;
using MFCSoftware.Models;
using System.Text;
using System.Windows.Media;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using MFCSoftware.Utils;
using MFCSoftware.Common;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MFCSoftware.ViewModels
{
    public class ChannelUserControlViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainVm;
        public static int SeriesPointNumber = 100;  //曲线显示点数
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
                SetProperty(ref _address, value);
                SetCommands();
            }
        }

        public string[] DisplayUnits { get; } = new string[] { "SCCM", "SLM", "%F.S" };

        private string _displayUnit;
        public string DisplayUnit
        {
            get => _displayUnit;
            set => SetProperty(ref _displayUnit, value);
        }

        public SolidColorBrush StatusColor { get; private set; } = new(Colors.Transparent);

        public BaseInformation BaseInfo { get; private set; }
        public DeviceVersion Version { get; private set; }
        public FormattedFlow FormattedFlow { get; private set; } = new();

        private uint _insertInterval = 30;
        public uint InsertInterval
        {
            get => _insertInterval;
            set
            {
                if (value >= 20)    //最小可以设置20毫秒
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
                string appName = _mainVm.AppName;
                return appName.Contains("MFC") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility TemperetureVisibility => 
            AppSettings.ReadTemperature? Visibility.Visible : Visibility.Collapsed;

        public SerialCommand<byte[]> ReadFlowBytes { get; private set; }
        public SerialCommand<byte[]> ReadBaseInfoBytes { get; private set; }
        public SerialCommand<byte[]> ReadVersion { get; private set; }
        public SerialCommand<byte[]> ClearAccuFlowBytes { get; private set; }
        public SerialCommand<byte[]> WriteFlowBytes { get; private set; }
        public SerialCommand<byte[]> WriteValveBytes { get; private set; }
        public SerialCommand<byte[]> ZeroPointCalibrationBytes { get; private set; }
        public SerialCommand<byte[]> FactoryRecoveryBytes { get; private set; }

        private void SetCommands()
        {
            //2022.04.20 读取流量再次修改为原指令
            if (AppSettings.ReadTemperature)
            {
                ReadFlowBytes = GetSerialCommandFromBytes(
                    bytes: new byte[] { 0x03, 0x00, 0x15, 0x00, 0x0C },
                    type: SerialCommandType.ReadFlow,
                    responseLength: 29);
            }
            else
            {
                ReadFlowBytes = GetSerialCommandFromBytes(
                    bytes: new byte[] { 0x03, 0x00, 0x16, 0x00, 0x0B },
                    type: SerialCommandType.ReadFlow,
                    responseLength: 27);
            }

            ReadBaseInfoBytes = GetSerialCommandFromBytes(
                bytes: new byte[] { 0x03, 0x00, 0x03, 0x00, 0x10 }, 
                type: SerialCommandType.BaseInfoData, 
                responseLength: 37);

            ReadVersion = GetSerialCommandFromBytes(
                bytes: new byte[] { 0x03, 0x00, 0x28, 0x00, 0x0A },
                type: SerialCommandType.ReadVersion,
                responseLength: 25);

            //修改为标准modbus协议 2021.09.01
            ClearAccuFlowBytes = GetSerialCommandFromBytes(
                bytes: new byte[] { 0x10, 0x00, 0x18, 0x00, 0x04, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 
                type: SerialCommandType.ClearAccuFlowData, 
                responseLength: 8);

            ZeroPointCalibrationBytes = GetSerialCommandFromBytes(
                bytes: new byte[] { 0x06, 0x00, 0x25, 0x00, 0x01 },
                type: SerialCommandType.ZeroPointCalibration);

            FactoryRecoveryBytes = GetSerialCommandFromBytes(
                bytes: new byte[] { 0x06, 0x00, 0x25, 0x00, 0x02 },
                type: SerialCommandType.FactoryRecovery);
        }

        public void SetBaseInfomation(BaseInformation info)
        {
            if (BaseInfo == null)
            {
                BaseInfo = new BaseInformation();
            }

            BaseInfo.SN = ConvertSN(info.SN);
            BaseInfo.Range = info.Range;
            BaseInfo.GasType = info.GasType;
            BaseInfo.Unit = info.Unit;
            DisplayUnit = info.Unit?.Unit;
            UpdateYAxisMaxValue(info.Range * 1.1D);

            OnPropertyChanged(nameof(BaseInfo));
            WhenSuccess();
        }

        public void SetVersion(DeviceVersion version)
        {
            Version = version;
            OnPropertyChanged(nameof(Version));
            WhenSuccess();
        }

        private string ConvertSN(string sn)
        {
            var builder = new StringBuilder();
            string[] splitter = sn.Split(' ');
            for (int index = 0; index < splitter.Length; index++)
            {
                builder.Append(splitter[index]);
                if ((index + 1) % 2 == 0 && index < splitter.Length - 1)
                    builder.Append(" ");
            }
            return builder.ToString();
        }

        public void SetFlow(FlowData flow, int bit)
        {
            var value = (BaseInfo?.Unit?.Unit, DisplayUnit) switch
            {
                ("SCCM", "SLM") => flow.CurrentFlow * 0.001,
                ("SLM", "SCCM") => flow.CurrentFlow * 1000,
                (_, "%F.S") => flow.CurrentFlow / BaseInfo.Range * 100,
                _ => flow.CurrentFlow
            };

            FormattedFlow.Origin = flow;
            FormattedFlow.Value = bit switch
            {
                0 => value.ToString("F2"),      //两位小数
                1 => value.ToString("F3"),      //三位小数
                2 => value.ToString("F0"),      //整数
                3 => value.ToString("F1"),      //一位小数
                _ => value.ToString()
            };
            
            OnPropertyChanged(nameof(FormattedFlow));
            WhenSuccess();
        }

        public void SetFlow(FlowData flow)
        {
            var value = (BaseInfo?.Unit?.Unit, DisplayUnit) switch
            {
                ("SCCM", "SLM") => flow.CurrentFlow * 0.001,
                ("SLM", "SCCM") => flow.CurrentFlow * 1000,
                (_, "%F.S") => flow.CurrentFlow / BaseInfo.Range * 100,
                _ => flow.CurrentFlow
            };
            
            FormattedFlow.Origin = flow;
            //2022.04.22 显示的小数点位数修改为根据量程确定
            FormattedFlow.Value = (BaseInfo?.Unit?.Unit, BaseInfo?.Range) switch
            {
                ("SCCM", int range) when range <= 100 => value.ToString("F2"),       //两位小数
                ("SCCM", int range) when range <= 1000 => value.ToString("F1"),      //一位小数
                ("SCCM", int range) when range <= 5000 => value.ToString("F0"),      //整数
                ("SLM", _) => value.ToString("F2"),
                _ => value.ToString()
            };

            OnPropertyChanged(nameof(FormattedFlow));
            WhenSuccess();
        }
        
        public void UpdateSeries(float value)
        {
            lock (SeriesPlotModel.SyncRoot)
            {
                var s = (OxyPlot.Series.LineSeries)SeriesPlotModel.Series[0];
                double x = DateTimeAxis.ToDouble(DateTime.Now);
                double y = value;
                s.Points.Add(new DataPoint(x, y));

                if (s.Points.Count > SeriesPointNumber)
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
                Position = AxisPosition.Bottom,
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
                Position = AxisPosition.Left,
                TitleFontSize = 16
            };
            SeriesPlotModel.Axes.Add(valueAxis);

            var line = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 1,
                MarkerType = MarkerType.None,
                MarkerStrokeThickness = 1,
                InterpolationAlgorithm = InterpolationAlgorithms.CatmullRomSpline
            };
            SeriesPlotModel.Series.Add(line);
            OnPropertyChanged(nameof(SeriesPlotModel));
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

        public void WhenTimeOut() => SetStatusColor(ReceivedStatus.Timeout);
        public void WhenSuccess() => SetStatusColor(ReceivedStatus.Success);
        public void WhenResolveFailed() => SetStatusColor(ReceivedStatus.ResolveFailed);

        private void SetStatusColor(ReceivedStatus status)
        {
            if (status == ReceivedStatus.Success)
            {
                StatusColor.Color = Colors.Green;
            }
            else if (status == ReceivedStatus.ResolveFailed)
            {
                StatusColor.Color = Colors.Yellow;
            }
            else if (status == ReceivedStatus.Timeout)
            {
                StatusColor.Color = Colors.Red;
            }

            OnPropertyChanged(nameof(StatusColor));
        }

        public void SetWriteFlowBytes()
        {
            //addr 0x06 0x00 0x21 0x00 0x00 [FLOW_1] {FLOW_2] {FLOW_3] {FLOW_4] CRCL CRCH
            int flowIntValue = ParseFloatToInt32(FlowValue * 100);
            WriteFlowBytes = new SerialCommandBuilder(SerialCommandType.SetFlow)
                .AppendAddress(Address)
                //.AppendBytes(new byte[] { 0x10, 0x00, 0x21, 0x00, 0x04, 0x08, 0x00, 0x00 }) //针对客户的修改 2021.08.17
                .AppendBytes(new byte[] { 0x10, 0x00, 0x21, 0x00, 0x03, 0x06, 0x00, 0x00 }) //修改为标准modbus协议 2021.09.01
                .AppendBytes(flowIntValue.ToHex())
                //.AppendBytes(new byte[] { 0x00, 0x00 }) //针对客户的修改 2021.08.17
                .AppendCrc16()
                .WithResponseLength(8)
                .Build(); //修改为标准modbus协议，响应长度为8 2021.09.01
        }

        public void SetWriteValveBytes()
        {
            //addr 0x06 0x00 0x21 0x00 0x03 [VALVE_VALUE_1] [VALVE_VALUE_2] CRCL CRCH
            int openIntValue = ParseFloatToInt32(ValveOpenValue * 100);
            WriteValveBytes = new SerialCommandBuilder(SerialCommandType.ValveControl)
                .AppendAddress(Address)
                //.AppendBytes(new byte[] { 0x06, 0x00, 0x21, 0x00, 0x03 })
                //.AppendBytes(new byte[] { 0x10, 0x00, 0x21, 0x00, 0x04, 0x08, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00 }) //针对客户的修改 2021.08.17
                .AppendBytes(new byte[] { 0x10, 0x00, 0x21, 0x00, 0x04, 0x08, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00 })
                .AppendBytes(openIntValue.ToHex().SubArray(2, 2))
                .AppendCrc16()
                .WithResponseLength(8)
                .Build();
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
                .WithResponseLength(responseLength)
                .Build();
        }
    }

    public enum ControlSelector
    {
        FlowValue,
        ValveOpenValue
    }
}
