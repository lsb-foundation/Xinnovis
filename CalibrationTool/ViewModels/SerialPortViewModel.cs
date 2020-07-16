using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using CommonLib.Mvvm;

namespace CalibrationTool.ViewModels
{
    public class SerialPortViewModel : BindableBase
    {
        private SerialPort serialPort = new SerialPort();

        public SerialPortViewModel()
        {
            InitializeSerialPortNameCollection();
        }

        #region 属性定义
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
        #endregion

        #region 默认列表定义
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
        #endregion

        public SerialPort GetSerialPortInstance()
        {
            if(serialPort == null)
            {
                serialPort = new SerialPort();
            }
            return serialPort;
        }

        private void InitializeSerialPortNameCollection()
        {
            foreach(string portName in SerialPort.GetPortNames())
            {
                PortNameCollection.Add(portName);
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
    }
}
