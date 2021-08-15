using CommonLib.Communication.Serial;
using GalaSoft.MvvmLight;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;

namespace AwesomeCommand.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly SerialPortInstance _instance;

        public SettingViewModel(SerialPortInstance instance)
        {
            _instance = instance;
        }

        public string PortName
        {
            get => _instance.SerialPort.PortName;
            set
            {
                _instance.SetPortName(value);
                RaisePropertyChanged();
            }
        }

        public int BaudRate
        {
            get => _instance.SerialPort.BaudRate;
            set
            {
                _instance.SerialPort.BaudRate = value;
                RaisePropertyChanged();
            }
        }

        public int DataBits
        {
            get => _instance.SerialPort.DataBits;
            set
            {
                _instance.SerialPort.DataBits = value;
                RaisePropertyChanged();
            }
        }

        public Parity Parity
        {
            get => _instance.SerialPort.Parity;
            set
            {
                _instance.SerialPort.Parity = value;
                RaisePropertyChanged();
            }
        }

        public StopBits StopBits
        {
            get => _instance.SerialPort.StopBits;
            set
            {
                _instance.SerialPort.StopBits = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> SerialPortNames { get; } = new ObservableCollection<string>();
        public List<int> SerialBaudRates { get; } = BaudRateCode.GetBaudRates();
        public List<int> SerialDataBits { get; } = new List<int>() { 5, 6, 7, 8 };
        public List<Parity> SerialParities { get; } = AdvancedSerialPort.GetParities();
        public List<StopBits> SerialStopBits { get; } = AdvancedSerialPort.GetStopBits();
    }
}
