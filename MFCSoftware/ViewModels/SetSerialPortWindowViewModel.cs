using GalaSoft.MvvmLight;
using System.Collections.Generic;
using CommonLib.Communication.Serial;
using System;
using System.IO.Ports;
using MFCSoftware.Utils;

namespace MFCSoftware.ViewModels
{
    public class SetSerialPortWindowViewModel : ViewModelBase
    {
        private readonly SerialPort _serialPort;
        public SetSerialPortWindowViewModel()
        {
            _serialPort = SerialPortInstance.GetSerialPortInstance();
            PortNames = AdvancedSerialPort.GetPortNames();
            BaudRates = BaudRateCode.GetBaudRates();
            PortName = _serialPort?.PortName;
        }

        public List<string> PortNames { get; private set; }
        public List<int> BaudRates { get; private set; }

        private string _portName;
        public string PortName
        {
            get => _portName;
            set => Set(ref _portName, value);
        }
        private int _baudRate = 9600;
        public int BaudRate
        {
            get => _baudRate;
            set => Set(ref _baudRate, value);
        }

        public void SetSerialPort()
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
                _serialPort.PortName = _portName;
                _serialPort.BaudRate = _baudRate;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
