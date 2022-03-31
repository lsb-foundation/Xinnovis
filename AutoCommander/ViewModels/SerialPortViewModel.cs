using AutoCommander.Common;
using CommonLib.Communication.Serial;
using CommonLib.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
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
        get => SerialInstance.Default.Instance.PortName;
        set
        {
            SerialInstance.Default.SetPortName(value);
            OnPropertyChanged();
        }
    }

    public int BaudRate
    {
        get => SerialInstance.Default.Instance.BaudRate;
        set
        {
            SerialInstance.Default.SetBaudRate(value);
            OnPropertyChanged();
        }
    }

    public bool IsOpen => SerialInstance.Default.Instance.IsOpen;
    public bool CanPortModify => !IsOpen;

    public ObservableCollection<string> SerialPortNames { get; }
    public List<int> SerialBaudRates { get; } = BaudRateCode.GetBaudRates();

    private void SwitchPort()
    {
        var instance = SerialInstance.Default.Instance;
        if (instance.IsOpen) instance.Close();
        else instance.Open();
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(CanPortModify));
    }

    public void TrySend<T>(T data)
    {
        try
        {
            var instance = SerialInstance.Default.Instance;
            if (!instance.IsOpen) instance.Open();
            OnPropertyChanged(nameof(IsOpen));
            OnPropertyChanged(nameof(CanPortModify));
            SerialInstance.Default.Send(data);
        }
        catch (Exception e)
        {
            LoggerHelper.WriteLog(e.Message, e);
            HandyControl.Controls.Growl.Warning(e.Message);
        }
    }
}
