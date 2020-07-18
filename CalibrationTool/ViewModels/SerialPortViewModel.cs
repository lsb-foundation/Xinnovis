using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Management;
using CalibrationTool.Models;
using CommonLib.Mvvm;

namespace CalibrationTool.ViewModels
{
    public class SerialPortViewModel : BindableBase, IMessageHandler<string>
    {
        #region Private field
        private SerialPort serialPort = new SerialPort();
        #endregion

        #region Constructions
        public SerialPortViewModel()
        {
            InitializeSerialPortNameCollection();
            OpenOrCloseCommand = new RelayCommand(o => OpenOrClosePort());
        }
        #endregion

        #region Properties
        public string PortName
        {
            get => serialPort.PortName;
            set
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
                serialPort.PortName = value;
                RaiseProperty();
            }
        }

        public int BaudRate
        {
            get => serialPort.BaudRate;
            set
            {
                serialPort.BaudRate = value;
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
            StopBits.None, StopBits.One, StopBits.OnePointFive, StopBits.Two
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
        public SerialPort GetSerialPortInstance()
        {
            if(serialPort == null)
            {
                serialPort = new SerialPort();
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
        private void InitializeSerialPortNameCollection()
        {
            foreach(string portName in SerialPort.GetPortNames())
            {
                PortNameCollection.Add(portName);
            }
        }

        private void OpenOrClosePort()
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    MessageHandler?.Invoke($"端口{serialPort.PortName}已关闭");
                }
                else
                {
                    serialPort.Open();
                    MessageHandler?.Invoke($"端口{serialPort.PortName}已打开");
                }
                RaiseProperty(nameof(OpenOrCloseString));
            }
            catch (Exception e)
            {
                MessageHandler?.Invoke("端口错误：" + e.Message);
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
