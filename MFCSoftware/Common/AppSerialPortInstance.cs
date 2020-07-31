using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Communication.Serial;

namespace MFCSoftware.Common
{
    public static class AppSerialPortInstance
    {
        private static AdvancedSerialPort _serialPort;

        static AppSerialPortInstance()
        {
            _serialPort = new AdvancedSerialPort();
        }

        public static AdvancedSerialPort GetSerialPortInstance()
        {
            if (_serialPort == null)
                _serialPort = new AdvancedSerialPort();
            return _serialPort;
        }
    }
}
