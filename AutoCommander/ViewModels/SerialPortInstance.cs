using AutoCommander.Properties;
using System.IO.Ports;
using System.Linq;

namespace AutoCommander.ViewModels
{
    public class SerialPortInstance
    {
        private readonly SerialPort _port;
        private readonly object _syncRoot;

        public SerialPortInstance()
        {
            _port = new SerialPort();
            _syncRoot = new object();
            string port = Settings.Default.PortName;
            if (SerialPort.GetPortNames().Any(pn => pn == port))
            {
                SetPortName(port);
            }
        }

        public SerialPort Port => _port;

        public void SetPortName(string portName)
        {
            lock (_syncRoot)
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
                _port.PortName = portName;

                Settings.Default.PortName = portName;
                Settings.Default.Save();
                Settings.Default.Reload();
            }
        }

        public void Send(string command)
        {
            lock (_syncRoot)
            {
                if (!_port.IsOpen)
                {
                    _port.Open();
                }
                _port.Write(command);
            }
        }
    }
}
