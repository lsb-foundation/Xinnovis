using MultipleDevicesMonitor.Properties;

namespace MultipleDevicesMonitor.ViewModels
{
    public class SettingsViewModel:ViewModelBase
    {
        public string Serial_PortName
        {
            get => Settings.Default.Serial_PortName;
            set
            {
                Settings.Default.Serial_PortName = value;
                Settings.Default.Save();
                RaiseProperty();
            }
        }

        public double Serial_BaudRate
        {
            get => Settings.Default.Serial_BaudRate;
            set
            {
                Settings.Default.Serial_BaudRate = value;
                Settings.Default.Save();
                RaiseProperty();
            }
        }

        public string YAxisTitle
        {
            get => Settings.Default.YAxisTitle;
            set
            {
                Settings.Default.YAxisTitle = value;
                Settings.Default.Save();
                RaiseProperty();
            }
        }

        public double TimerInterval
        {
            get => Settings.Default.TimerInterval;
            set
            {
                Settings.Default.TimerInterval = value;
                Settings.Default.Save();
                RaiseProperty();
            }
        }

        public int DisplayPointsNumber
        {
            get => Settings.Default.DisplayPointsNumber;
            set
            {
                Settings.Default.DisplayPointsNumber = value;
                Settings.Default.Save();
                RaiseProperty();
            }
        }
    }
}
