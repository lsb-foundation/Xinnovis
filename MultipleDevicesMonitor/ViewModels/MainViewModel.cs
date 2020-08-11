using CommonLib.Mvvm;
using MultipleDevicesMonitor.Properties;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Media;

namespace MultipleDevicesMonitor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Timer timer;
        private readonly List<int> addrList = new List<int>();
        
        public MainViewModel()
        {
            timer = new Timer
            {
                AutoReset = true,
                Interval = Settings.Default.TimerInterval
            };
            timer.Elapsed += Timer_Elapsed;
            SetPlotModel();
        }

        public bool DeviceCanAdd { get => !addrList.Contains(_addr); }
        public PlotModel SeriesPlotModel { get; private set; }

        private int _addr;
        public int Address
        {
            get => _addr;
            set => SetProperty(ref _addr, value);
        }

        private bool _isStopped;
        public bool IsStopped
        {
            get => _isStopped;
            set
            {
                if (value) timer.Stop();
                else timer.Start();
                SetProperty(ref _isStopped, value);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (SeriesPlotModel.SyncRoot)
            {
                Update();
            }

            SeriesPlotModel.InvalidatePlot(true);
        }

        private void Update()
        {
            Random random = new Random();

            for (int i = 0; i < SeriesPlotModel.Series.Count; i++)
            {
                var s = (LineSeries)SeriesPlotModel.Series[i];

                double x = DateTimeAxis.ToDouble(DateTime.Now);
                double y = Math.Sin(random.Next(100));
                s.Points.Add(new DataPoint(x, y));

                if (s.Points.Count >= Settings.Default.DisplayPointsNumber)
                    s.Points.RemoveAt(0);
            }
        }

        private void SetPlotModel()
        {
            SeriesPlotModel = new PlotModel
            {
                Background = OxyColors.Black,
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.White,
                LegendPosition = LegendPosition.LeftTop
            };

            var dateTimeAxis = new DateTimeAxis
            {
                Title = "时间",
                Position = AxisPosition.Bottom,
                IntervalType = DateTimeIntervalType.Seconds,
                IntervalLength = 50,
            };
            SeriesPlotModel.Axes.Add(dateTimeAxis);

            var valueAxis = new LinearAxis
            {
                Title = Settings.Default.YAxisTitle,
                Position = AxisPosition.Right,
            };
            SeriesPlotModel.Axes.Add(valueAxis);

            RaiseProperty(nameof(SeriesPlotModel));
            timer.Start();
        }

        public void UpdateAxisTitle()
        {
            foreach(var axis in SeriesPlotModel.Axes)
            {
                if (axis.GetType().Name == "LinearAxis")
                {
                    axis.Title = Settings.Default.YAxisTitle;
                }
            }
        }

        public void AddNewDevice()
        {
            if (DeviceCanAdd)
            {
                var line = new LineSeries
                {
                    Title = "设备: " + _addr,
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerStrokeThickness = 2.5,
                    InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline
                };

                SeriesPlotModel.Series.Add(line);
                addrList.Add(_addr);
            }
        }
    }
}
