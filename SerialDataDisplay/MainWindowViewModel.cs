using CommonLib.Communication.Serial;
using CommonLib.Mvvm;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Configurations;

namespace SerialDataDisplay
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel()
        {
            Serial = new SerialPort()
            {
                DataBits = 8,
                BaudRate = 9600,
                Parity = Parity.Even,
                StopBits = StopBits.One
            };

            PortNameCollection = AdvancedSerialPort.GetPortNames();
            BaudRateCollection = BaudRateCode.GetBaudRates();

            var values = new ChartValues<ObservableValue>();
            for(int index = 0; index < 100; index++)
            {
                var value = new ObservableValue(0.0f);
                values.Add(value);
            }

            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    AreaLimit = -10,
                    Values = values
                }
            };
        }

        public List<string> PortNameCollection { get; private set; }
        public List<int> BaudRateCollection { get; private set; }
        public List<int> DataBitsCollection { get; private set; } = new List<int>() { 5, 6, 7, 8 };
        public List<Parity> ParityCollection { get; private set; }
            = new List<Parity>() { Parity.None, Parity.Even, Parity.Odd, Parity.Mark, Parity.Space };
        public List<StopBits> StopBitsCollection { get; private set; }
            = new List<StopBits>() { StopBits.One, StopBits.OnePointFive, StopBits.Two };

        public SerialPort Serial { get; }

        public string PortName
        {
            get => Serial.PortName;
            set
            {
                if (Serial.IsOpen)
                    Serial.Close();

                Serial.PortName = value;
                RaiseProperty();
            }
        }

        public int BaudRate
        {
            get => Serial.BaudRate;
            set
            {
                Serial.BaudRate = value;
                RaiseProperty();
            }
        }

        public int DataBits
        {
            get => Serial.DataBits;
            set
            {
                Serial.DataBits = value;
                RaiseProperty();
            }
        }

        public Parity Parity
        {
            get => Serial.Parity;
            set
            {
                Serial.Parity = value;
                RaiseProperty();
            }
        }

        public StopBits StopBits
        {
            get => Serial.StopBits;
            set
            {
                Serial.StopBits = value;
                RaiseProperty();
            }
        }

        public SeriesCollection SeriesCollection { get; set; }

        private float _currentVolte;
        public float CurrentVolte
        {
            get => _currentVolte;
            set => SetProperty(ref _currentVolte, value);
        }

        public Func<double, string> VolteLabelsFormatter { get => v => string.Format("{0:N2}", v); } 
    }
}
