using System.IO.Ports;

namespace AwesomeCommand.ViewModels
{
    public class SerialPortInstance
    {
        private readonly SerialPort _port;
        private readonly object _syncRoot;

        public SerialPortInstance()
        {
            _port = new SerialPort();
            _syncRoot = new object();
        }

        public SerialPort SerialPort => _port;

        public void SetPortName(string portName)
        {
            lock (_syncRoot)
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
                _port.PortName = portName;
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
