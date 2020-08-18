using CommonLib.Communication.Serial;
using CommonLib.Mvvm;
using CommonLib.DbUtils;
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
using System.IO;

namespace SerialDataDisplay
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly string dbFile = "db.sqlite";
        private readonly string tableName = "tb_values";
        private readonly SqliteUtils utils;

        public MainWindowViewModel()
        {
            var dbFileName = Path.Combine(Environment.CurrentDirectory, dbFile);
            var connectionString = $"data source = {dbFileName}";
            utils = new SqliteUtils(connectionString);
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

        private bool _controlEnable = true;
        public bool ControlEnabled
        {
            get => _controlEnable;
            set => SetProperty(ref _controlEnable, value);
        }

        private SerialCommand _serialCommand;
        public SerialCommand CurrentCommand
        {
            get => _serialCommand;
            set => SetProperty(ref _serialCommand, value);
        }

        public SeriesCollection SeriesCollection { get; set; }

        private float _currentValue;
        public float CurrentValue
        {
            get => _currentValue;
            set => SetProperty(ref _currentValue, value);
        }

        public DateTime LastestStartTime { get; set; }

        public Func<double, string> ValueLabelsFormatter { get => v => string.Format("{0:N2}", v); } 

        public void InsertValue(float value)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"INSERT INTO {tableName}(collect_time, value) VALUES (")
                .Append("strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'),")
                .Append($"{value});");
            string sql = sqlBuilder.ToString();
            utils.ExecuteNonQuery(sql);
        }

        public List<TableValue> QueryValues()
        {
            var lastestStartTimeStr = LastestStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string sql = $"SELECT * FROM {tableName} WHERE collect_time > '{lastestStartTimeStr}';";
            List<TableValue> values = new List<TableValue>();
            var reader = utils.ExecuteQuery(sql);
            while (reader.Read())
            {
                try
                {
                    var value = new TableValue();
                    value.Time = reader.GetDateTime(0);
                    value.Value = reader.GetFloat(1);
                    values.Add(value);
                }
                catch { }
            }
            return values;
        }

        private void InitializeTables()
        {
            var tableTypes = new Dictionary<string, string>()
            {
                {"collect_time", "datetime" },
                {"value", "float" }
            };
            utils.CreateTableIfNotExists(tableName, tableTypes);
            utils.ClearTable(tableName);
        }
    }

    public class TableValue
    {
        public DateTime Time { get; set; }
        public float Value { get; set; }
    }
}
