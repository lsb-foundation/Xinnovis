﻿using GalaSoft.MvvmLight;
using System.Collections.Generic;
using CommonLib.Communication.Serial;
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
            _serialPort.BaudRate = 9600;
        }

        public List<string> PortNames { get; private set; }
        public List<int> BaudRates { get; private set; }

        public string PortName
        {
            get => _serialPort.PortName;
            set
            {
                if (PortNames.Contains(value))
                {   //从数据库中读取的串口号可能在本机已经改变
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.PortName = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int BaudRate
        {
            get => _serialPort.BaudRate;
            set
            {
                _serialPort.BaudRate = value;
                RaisePropertyChanged();
            }
        }

        public int SeriesPointNumber
        {
            get => ChannelUserControlViewModel.SeriesPointNumber;
            set => Set(ref ChannelUserControlViewModel.SeriesPointNumber, value);
        }
    }
}
