using AutoCommander.Properties;
using System.IO.Ports;
using System.Linq;

namespace AutoCommander.Common;

public class SerialPortInstance
{
    private static SerialPortInstance _default;
    private readonly SerialPort _instance = new() { WriteTimeout = 500 };

    private SerialPortInstance()
    {
        string port = Settings.Default.PortName;
        int baudRate = Settings.Default.BaudRate;
        if (SerialPort.GetPortNames().Any(pn => pn == port))
        {
            SetPortName(port);
            SetBaudRate(baudRate);
        }
    }

    public static SerialPortInstance Default
    {
        get
        {
            if (_default == null) _default = new();
            return _default;
        }
    }

    public SerialPort Instance => _instance;

    public void SetPortName(string portName )
    {
        lock (_instance)
        {
            if (_instance.IsOpen)
            {
                _instance.Close();
            }
            _instance.PortName = portName;
            SettingsUtils.Save("PortName", portName);
        }
    }

    public void SetBaudRate(int baudRate)
    {
        _instance.BaudRate = baudRate;
        SettingsUtils.Save("BaudRate", baudRate);
    }

    public void Send<T>(T data)
    {
        if (!_instance.IsOpen) return;
        lock (_instance)
        {
            if (data is string text) _instance.Write(text);
            else if (data is byte[] binData) _instance.Write(binData, 0, binData.Length);
        }
    }
}
