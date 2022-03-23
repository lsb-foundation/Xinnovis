using MultipleDevicesMonitor.Properties;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MultipleDevicesMonitor.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly MainViewModel _mainVm;

        public SettingsViewModel(MainViewModel mainVm)
        {
            _mainVm = mainVm;
        }

        public string YAxisTitle
        {
            get => Settings.Default.YAxisTitle;
            set
            {
                Settings.Default.YAxisTitle = value;
                SaveSettings();
                _mainVm.UpdateAxisTitle();
                OnPropertyChanged();
            }
        }

        public double TimerInterval
        {
            get => Settings.Default.TimerInterval;
            set
            {
                Settings.Default.TimerInterval = value;
                SaveSettings();
                _mainVm.UpdateTimerInterval(value);
                OnPropertyChanged();
            }
        }

        public int DisplayPointsNumber
        {
            get => Settings.Default.DisplayPointsNumber;
            set
            {
                Settings.Default.DisplayPointsNumber = value;
                SaveSettings();
                OnPropertyChanged();
            }
        }

        private void SaveSettings()
        {
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
