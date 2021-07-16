using CommonLib.Extensions;
using CommonLib.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace AutoCalibrationTool.ViewModel
{
    public class PortViewModel : ViewModelBase
    {
        public PortViewModel()
        {
            _serial.BaudRate = 115200;
            _serial.Parity = Parity.None;
            _serial.DataBits = 8;
            _serial.StopBits = StopBits.One;
            _serial.DataReceived += SerialDataReceived;
            OpenOrClosePortCommand = new RelayCommand(() => OpenOrClosePort());
        }

        #region Fields
        private readonly SerialPort _serial = new SerialPort();
        #endregion

        #region Events
        public event Action<byte[]> OnDataReceived;
        #endregion

        #region Properties
        public string PortName
        {
            get => _serial.PortName;
            set => _serial.PortName = value;
        }

        public string[] SerialPortNames => SerialPort.GetPortNames();
        public bool IsOpen => _serial.IsOpen;
        public bool IsClosed => !_serial.IsOpen;
        public bool PortButtonEnabled => ViewModelLocator.Main.Mode == Models.CalibrationMode.Stop;
        #endregion

        #region Commands
        public RelayCommand OpenOrClosePortCommand { get; }
        #endregion

        #region Methods
        public bool Send(string content)
        {
            if (_serial.IsOpen)
            {
                _serial.Write(content);
                LoggerHelper.WriteLog($"Send: {content}");
                return true;
            }
            return false;
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int len = _serial.BytesToRead;
            var bytes = new byte[len];
            _serial.Read(bytes, 0, len);
            OnDataReceived?.Invoke(bytes);
            Task.Run(() => LoggerHelper.WriteLog($"Received: {bytes.ToHexString()}")); //新线程写日志
        }

        private void OpenOrClosePort()
        {
            if (string.IsNullOrEmpty(_serial.PortName)) return;
            if (SerialPortNames.All(n => n != _serial.PortName)) return;
            if (_serial.IsOpen) _serial.Close();
            else _serial.Open();
            RefreshProperty();
        }

        private void RefreshProperty()
        {
            RaisePropertyChanged(nameof(IsOpen));
            RaisePropertyChanged(nameof(IsClosed));
            ViewModelLocator.Main.UpdateButtonEnableStatus();
        }

        public void UpdatePortButtonStatus()
        {
            RaisePropertyChanged(nameof(PortButtonEnabled));
        }
        #endregion
    }
}
