using CommonLib.MfcUtils;
using GalaSoft.MvvmLight;
using System.IO.Ports;
using System.Linq;

namespace MFCSoftwareForCUP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly SerialPort _serialPort;
        private int maxDeviceCount;
        private int addressToAdd;
        #endregion

        public MainViewModel()
        {
            _serialPort = SerialPortInstance.GetSerialPortInstance();
            _serialPort.BaudRate = 9600;
        }

        public string PortName
        {
            get => _serialPort.PortName;
            set
            {
                if (PortNames.Any(n => n == value))
                {
                    _serialPort.PortName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int MaxDeviceCount
        {
            get => maxDeviceCount;
            set => Set(ref maxDeviceCount, value);
        }

        public int AddressToAdd
        {
            get => addressToAdd;
            set => Set(ref addressToAdd, value);
        }

        public string[] PortNames => SerialPort.GetPortNames();
    }
}
