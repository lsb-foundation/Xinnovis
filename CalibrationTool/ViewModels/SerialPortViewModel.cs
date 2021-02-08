using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Management;
using CalibrationTool.Models;
using CalibrationTool.Utils;
using CommonLib.Mvvm;
using CommonLib.Communication.Serial;

namespace CalibrationTool.ViewModels
{
    public class SerialPortViewModel : ViewModelBase, IMessageHandler<string>
    {
        #region Private field
        private AdvancedSerialPort serialPort;
        #endregion

        #region Constructions
        public SerialPortViewModel()
        {
            InitializeSerialPort();
            AdvancedSerialPort.GetPortNames().ForEach(n => PortNameCollection.Add(n));
            OpenOrCloseCommand = new RelayCommand(o =>
            {
                if (serialPort.IsOpen) TryToClosePort();
                else TryToOpenPort();
            });
        }
        #endregion

        #region Properties
        public string PortName
        {
            get => serialPort.PortName;
            set
            {
                TryToClosePort();
                serialPort.PortName = value;
                ConfigManager.Serial_PortName = value;
                RaiseProperty();
            }
        }

        public int BaudRate
        {
            get => serialPort.BaudRate;
            set
            {
                serialPort.BaudRate = value;
                ConfigManager.Serial_BaudRate = value;
                RaiseProperty();
            }
        }

        public int DataBits
        {
            get => serialPort.DataBits;
            set
            {
                serialPort.DataBits = value;
                RaiseProperty();
            }
        }

        public Parity Parity
        {
            get => serialPort.Parity;
            set
            {
                serialPort.Parity = value;
                RaiseProperty();
            }
        }

        public StopBits StopBits
        {
            get => serialPort.StopBits;
            set
            {
                serialPort.StopBits = value;
                RaiseProperty();
            }
        }

        public string OpenOrCloseString
        {
            get => serialPort.IsOpen ? "关闭" : "打开";
        }

        public ObservableCollection<string> PortNameCollection { get; private set; } 
            = new ObservableCollection<string>();

        public List<int> BaudRateCollection { get; private set; } = BaudRateCode.GetBaudRates();

        public List<int> DataBitsCollection { get; private set; } = new List<int>() { 5, 6, 7, 8 };

        public List<Parity> ParityCollection { get => AdvancedSerialPort.GetParities(); }

        public List<StopBits> StopBitsCollection { get => AdvancedSerialPort.GetStopBits(); }

        //自动换行
        private bool _autoAddNewLine = true;
        public bool AutoAddNewLine
        {
            get => _autoAddNewLine;
            set => SetProperty(ref _autoAddNewLine, value);
        }

        //接收类型：ASCII/Hex
        private CommunicationDataType _receivedType = CommunicationDataType.ASCII;
        public CommunicationDataType ReceivedType
        {
            get => _receivedType;
            set => SetProperty(ref _receivedType, value);
        }

        //发送类型：ASCII/Hex
        private CommunicationDataType _sendType = CommunicationDataType.ASCII;
        public CommunicationDataType SendType
        {
            get => _sendType;
            set => SetProperty(ref _sendType, value);
        }
        #endregion

        #region Command
        public RelayCommand OpenOrCloseCommand { get; private set; }
        public Action<string> MessageHandler { get; set; }
        #endregion

        #region Public methods
        public AdvancedSerialPort GetSerialPortInstance()
        {
            if(serialPort == null)
            {
                serialPort = new AdvancedSerialPort();
            }
            return serialPort;
        }

        public void TryToOpenPort()
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    MessageHandler?.Invoke($"端口{serialPort.PortName}已打开");
                    RaiseProperty(nameof(OpenOrCloseString));
                }
            }
            catch(Exception e)
            {
                MessageHandler?.Invoke("端口错误：" + e.Message);
            }
        }
        #endregion

        #region Private methods
        private void InitializeSerialPort()
        {
            serialPort = new AdvancedSerialPort();
            serialPort.BaudRate = ConfigManager.Serial_BaudRate;
            if (!string.IsNullOrEmpty(ConfigManager.Serial_PortName))
                serialPort.PortName = ConfigManager.Serial_PortName;
        }

        private void TryToClosePort()
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    MessageHandler?.Invoke($"端口{serialPort.PortName}已关闭");
                    RaiseProperty(nameof(OpenOrCloseString));
                }
            }
            catch(Exception e)
            {
                MessageHandler?.Invoke("端口错误：" + e.Message);
            }
        }
        #endregion

        /// <summary>
        /// 获取串口号对应的设备完整名称列表，该方法暂时未使用。
        /// </summary>
        /// <returns></returns>
        public string[] GetSerialPortFullNames()
        {
            List<string> fullNames = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    var nameValue = hardInfo.Properties["Name"].Value;
                    if (nameValue == null) continue;
                    foreach (string portName in SerialPort.GetPortNames())
                    {
                        if (nameValue.ToString().Contains(portName))
                            fullNames.Add(nameValue.ToString());
                    }
                }
            }
            return fullNames.ToArray();
        }
    }
}
