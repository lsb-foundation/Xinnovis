using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommonLib.Communication.Serial;
using MultipleDevicesMonitor.Properties;
using CommonLib.Mvvm;

namespace MultipleDevicesMonitor.ViewModels
{
    public class SerialViewModel : ViewModelBase
    {
        private readonly SerialPort com;
        private readonly object syncObject = new object();

        public SerialViewModel()
        {
            com = new SerialPort();
            com.DataReceived += Com_DataReceived;
            PortNames = AdvancedSerialPort.GetPortNames();
            BaudRates = BaudRateCode.GetBaudRates();
        }

        public event Action<byte[]> DataRevieved;

        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            lock (syncObject)
            {
                List<byte> receivedBytes = new List<byte>();
                while (com.BytesToRead > 0)
                {
                    int count = com.BytesToRead;
                    byte[] buffer = new byte[count];
                    com.Read(buffer, 0, count);
                    receivedBytes.AddRange(buffer);
                    Thread.Sleep(5);
                }
                DataRevieved?.Invoke(receivedBytes.ToArray());
            }
        }

        public void Send(byte[] data)
        {
            if (!com.IsOpen)
                com.Open();
            com.Write(data, 0, data.Length);
        }

        public List<string> PortNames { get; }
        public List<int> BaudRates { get; }

        public string PortName
        {
            get
            {
                com.PortName = Settings.Default.Serial_PortName;
                return com.PortName;
            }
            set
            {
                try
                {
                    if (com.IsOpen)
                        com.Close();
                    com.PortName = value;
                    Settings.Default.Serial_PortName = value;
                    SaveSettings();
                    RaiseProperty();
                }
                catch { }
            }
        }

        public int BaudRate
        {
            get
            {
                com.BaudRate = Settings.Default.Serial_BaudRate;
                return com.BaudRate;
            }
            set
            {
                com.BaudRate = value;
                Settings.Default.Serial_BaudRate = value;
                SaveSettings();
                RaiseProperty();
            }
        }

        public SerialPort GetSerialPortInstance()
        {
            return com;
        }

        private void SaveSettings()
        {
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
