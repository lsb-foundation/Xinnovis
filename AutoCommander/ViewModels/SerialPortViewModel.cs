using AutoCommander.Common;
using CommonLib.Communication.Serial;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;

namespace AutoCommander.ViewModels;

public class SerialPortViewModel : ObservableObject
{
    public SerialPortViewModel()
    {
        SerialPortNames = new ObservableCollection<string>();
        SwitchPortCommand = new RelayCommand(SwitchPort);
        foreach (string portName in SerialPort.GetPortNames())
        {
            SerialPortNames.Add(portName);
        }
        if (!SerialPortNames.Contains(PortName))
        {
            PortName = SerialPortNames[0];
        }
    }

    public RelayCommand SwitchPortCommand { get; }

    public string PortName
    {
        get => SerialPortInstance.Default.Instance.PortName;
        set
        {
            SerialPortInstance.Default.SetPortName(value);
            OnPropertyChanged();
        }
    }

    public int BaudRate
    {
        get => SerialPortInstance.Default.Instance.BaudRate;
        set
        {
            SerialPortInstance.Default.SetBaudRate(value);
            OnPropertyChanged();
        }
    }

    public bool IsOpen => SerialPortInstance.Default.Instance.IsOpen;
    public bool CanPortModify => !IsOpen;

    public ObservableCollection<string> SerialPortNames { get; }
    public List<int> SerialBaudRates { get; } = BaudRateCode.GetBaudRates();

    private void SwitchPort()
    {
        var instance = SerialPortInstance.Default.Instance;
        if (instance.IsOpen) instance.Close();
        else instance.Open();
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(CanPortModify));
    }

    public void TrySend<T>(T data)
    {
        var instance = SerialPortInstance.Default.Instance;
        if (!instance.IsOpen) instance.Open();
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(CanPortModify));
        SerialPortInstance.Default.Send(data);
    }
}
