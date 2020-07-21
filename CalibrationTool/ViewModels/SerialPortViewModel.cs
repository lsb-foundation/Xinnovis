using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Management;
using CalibrationTool.Models;
using CalibrationTool.Utils;
using CommonLib.Mvvm;
using CommonLib.Communication;

namespace CalibrationTool.ViewModels
{
    public class SerialPortViewModel : BindableBase, IMessageHandler<string>
    {
        #region Private field
        private AdvancedSerialPort serialPort;
        #endregion

        #region Constructions
        public SerialPortViewModel()
        {
            InitializeSerialPort();
            InitializeSerialPortNameCollection();
            OpenOrCloseCommand = new RelayCommand(o =>
            {
                if (serialPort.IsOpen) ClosePort();
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
                ClosePort();
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

        public ObservableCollection<string> PortNameCollection { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<int> BaudRateCollection { get; set; } = new ObservableCollection<int>()
        {
            9600, 19200, 38400, 57600, 115200
        };

        public ObservableCollection<int> DataBitsCollection { get; set; } = new ObservableCollection<int>()
        {
            5, 6, 7, 8
        };

        public ObservableCollection<Parity> ParityCollection { get; set; } = new ObservableCollection<Parity>()
        {
            Parity.None, Parity.Even, Parity.Odd, Parity.Mark, Parity.Space
        };

        public ObservableCollection<StopBits> StopBitsCollection { get; set; } = new ObservableCollection<StopBits>()
        {
            StopBits.One, StopBits.OnePointFive, StopBits.Two
        };

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
        public RelayCommand OpenOrCloseCommand { get; set; }

        /// <summary>
        /// 将该类中所有产生的消息转交给外部处理
        /// </summary>
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

        private void InitializeSerialPortNameCollection()
        {
            foreach(string portName in SerialPort.GetPortNames())
            {
                PortNameCollection.Add(portName);
            }
        }

        private void ClosePort()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                MessageHandler?.Invoke($"端口{serialPort.PortName}已关闭");
                RaiseProperty(nameof(OpenOrCloseString));
            }
        }

        /// <summary>
        /// 获取串口号对应的设备完整名称列表，该方法暂时未使用。
        /// </summary>
        /// <returns></returns>
        private string[] GetSerialPortFullNames()
        {
            List<string> fullNames = new List<string>();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    object nameValue = hardInfo.Properties["Name"].Value;
                    if (nameValue != null && nameValue.ToString().Contains("COM"))
                    {
                        fullNames.Add(nameValue.ToString());
                    }
                }
                searcher.Dispose();
            }
            return fullNames.ToArray();
        }
        #endregion
    }
}
