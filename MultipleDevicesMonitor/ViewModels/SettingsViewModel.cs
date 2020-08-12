using MultipleDevicesMonitor.Properties;

namespace MultipleDevicesMonitor.ViewModels
{
    public class SettingsViewModel:ViewModelBase
    {
        public string YAxisTitle
        {
            get => Settings.Default.YAxisTitle;
            set
            {
                Settings.Default.YAxisTitle = value;
                SaveSettings();
                var main = GetViewModelInstance<MainViewModel>() as MainViewModel;
                main.UpdateAxisTitle();
                RaiseProperty();
            }
        }

        public double TimerInterval
        {
            get => Settings.Default.TimerInterval;
            set
            {
                Settings.Default.TimerInterval = value;
                SaveSettings();
                var main = GetViewModelInstance<MainViewModel>() as MainViewModel;
                main.UpdateTimerInterval(value);
                RaiseProperty();
            }
        }

        public int DisplayPointsNumber
        {
            get => Settings.Default.DisplayPointsNumber;
            set
            {
                Settings.Default.DisplayPointsNumber = value;
                SaveSettings();
                RaiseProperty();
            }
        }

        private void SaveSettings()
        {
            Settings.Default.Save();
            Settings.Default.Reload();
        }
    }
}
