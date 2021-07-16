using CommonLib.Communication.Serial;
using GalaSoft.MvvmLight;
using Dapper;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace SerialDataDisplay
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly string _connectionString;
        private readonly string dbFile = "db.sqlite";

        public MainWindowViewModel()
        {
            var dbFileName = Path.Combine(Environment.CurrentDirectory, dbFile);
            _connectionString = $"data source = {dbFileName}";
            InitializeTables();

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
        public List<Parity> ParityCollection { get => AdvancedSerialPort.GetParities(); }
        public List<StopBits> StopBitsCollection { get => AdvancedSerialPort.GetStopBits(); }
        public List<SerialCommand> CommandList { get => SerialCommand.GetCommands(); }

        public SerialPort Serial { get; }

        public string PortName
        {
            get => Serial.PortName;
            set
            {
                if (Serial.IsOpen)
                    Serial.Close();

                Serial.PortName = value;
                RaisePropertyChanged();
            }
        }

        public int BaudRate
        {
            get => Serial.BaudRate;
            set
            {
                Serial.BaudRate = value;
                RaisePropertyChanged();
            }
        }

        public int DataBits
        {
            get => Serial.DataBits;
            set
            {
                Serial.DataBits = value;
                RaisePropertyChanged();
            }
        }

        public Parity Parity
        {
            get => Serial.Parity;
            set
            {
                Serial.Parity = value;
                RaisePropertyChanged();
            }
        }

        public StopBits StopBits
        {
            get => Serial.StopBits;
            set
            {
                Serial.StopBits = value;
                RaisePropertyChanged();
            }
        }

        private bool _controlEnable = true;
        public bool ControlEnabled
        {
            get => _controlEnable;
            set => Set(ref _controlEnable, value);
        }

        private SerialCommand _serialCommand;
        public SerialCommand CurrentCommand
        {
            get => _serialCommand;
            set => Set(ref _serialCommand, value);
        }

        public SeriesCollection SeriesCollection { get; set; }

        private float _currentValue;
        public float CurrentValue
        {
            get => _currentValue;
            set => Set(ref _currentValue, value);
        }

        public DateTime LastestStartTime { get; set; }

        public Func<double, string> ValueLabelsFormatter { get => v => string.Format("{0:N2}", v); } 

        public void InsertValue(float value)
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.ExecuteAsync(
                    "insert into tb_values(collect_time, value) values(@Time, @Value)", 
                    new TableValue 
                    { 
                        Time = DateTime.Now, 
                        Value = value 
                    });
            }
        }

        public async Task<List<TableValue>> QueryValuesAsync()
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                var values = await connection.QueryAsync<TableValue>(
                    "select collect_time as Time, value as Value from tb_values where collect_time > @time", 
                    new { time = LastestStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff") });
                return values.ToList();
            }
        }

        private void InitializeTables()
        {
            using (IDbConnection connection = new SQLiteConnection(_connectionString))
            {
                connection.Execute("create table if not exists tb_values(collect_time datetime, value float);");
                connection.Execute("delete from tb_values where 1=1;");
            }
        }
    }

    public class TableValue
    {
        public DateTime Time { get; set; }
        public float Value { get; set; }
    }
}
