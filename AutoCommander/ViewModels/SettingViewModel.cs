using AutoCommander.Properties;
using CommonLib.Communication.Serial;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;

namespace AutoCommander.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private readonly SerialPortInstance _instance;

        public SettingViewModel(SerialPortInstance instance)
        {
            _instance = instance;
            SerialPortNames = new ObservableCollection<string>();
            foreach (string portName in SerialPort.GetPortNames())
            {
                SerialPortNames.Add(portName);
            }
            if (!SerialPortNames.Contains(_instance.Port.PortName))
            {
                _instance.Port.PortName = SerialPortNames[0];
            }
            LoadSettings();
        }

        public string PortName
        {
            get => _instance.Port.PortName;
            set
            {
                _instance.SetPortName(value);
                RaisePropertyChanged();
            }
        }

        public int BaudRate
        {
            get => _instance.Port.BaudRate;
            set
            {
                _instance.Port.BaudRate = value;
                RaisePropertyChanged();
            }
        }

        public int DataBits
        {
            get => _instance.Port.DataBits;
            set
            {
                _instance.Port.DataBits = value;
                RaisePropertyChanged();
            }
        }

        public Parity Parity
        {
            get => _instance.Port.Parity;
            set
            {
                _instance.Port.Parity = value;
                RaisePropertyChanged();
            }
        }

        public StopBits StopBits
        {
            get => _instance.Port.StopBits;
            set
            {
                _instance.Port.StopBits = value;
                RaisePropertyChanged();
            }
        }

        public List<string> AutoUiFiles { get; } = LoadAutoUiFiles();

        public string SelectedAutoUiFile { get; set; }

        public ObservableCollection<string> SerialPortNames { get; }
        public List<int> SerialBaudRates { get; } = BaudRateCode.GetBaudRates();
        public List<int> SerialDataBits { get; } = new List<int>() { 5, 6, 7, 8 };
        public List<Parity> SerialParities { get; } = AdvancedSerialPort.GetParities();
        public List<StopBits> SerialStopBits { get; } = AdvancedSerialPort.GetStopBits();

        private void LoadSettings()
        {
            string file = Settings.Default.AutoUiFile;
            if (AutoUiFiles.Contains(file))
            {
                SelectedAutoUiFile = file;
            }
        }

        public static List<string> LoadAutoUiFiles()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./autoui");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string[] files = Directory.GetFiles(path);

            return files
                .Select(f => new FileInfo(f))
                .Where(f => f.Extension.ToLower() == ".xml")
                .OrderBy(f => f.Name)
                .Select(f => f.Name)
                .ToList();
        }
    }
}
