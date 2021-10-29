using MFCSoftware.Utils;
using GalaSoft.MvvmLight;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace MFCSoftwareForCUP.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields
        private readonly DateTime _startTime = DateTime.Now;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);   //用于线程间串口资源共享同步的信号量
        private readonly SerialPort _serialPort;
        private int maxDeviceCount;
        private int addressToAdd;
        #endregion

        public MainViewModel()
        {
            _serialPort = SerialPortInstance.GetSerialPortInstance();
            _serialPort.BaudRate = 9600;
        }

        public SemaphoreSlim Semaphore => _semaphore;

        public string PortName
        {
            get => _serialPort.PortName;
            set => SetPortName(value);
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

        public DateTime AppStartTime => _startTime;

        private async void SetPortName(string port)
        {
            if (PortNames.Any(n => n == port))
            {
                await _semaphore.WaitAsync();
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.PortName = port;
                _ = _semaphore.Release();
                RaisePropertyChanged(nameof(PortName));
            }
        }
    }
}
