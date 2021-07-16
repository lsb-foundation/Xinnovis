using MultipleDevicesMonitor.Properties;
using GalaSoft.MvvmLight;

namespace MultipleDevicesMonitor.ViewModels
{
    public class SettingsViewModel : ViewModelBase
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
            }
        }

        public int DisplayPointsNumber
        {
            get => Settings.Default.DisplayPointsNumber;
            set
            {
                Settings.Default.DisplayPointsNumber = value;
                SaveSettings();
                RaisePropertyChanged();
            }
        }

        private void SaveSettings()
        {
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
