using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Mvvm;

namespace MultipleDevicesMonitor.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        private static MainViewModel mainViewModel;
        private static SettingsViewModel settingsViewModel;

        static ViewModelBase()
        {
            mainViewModel = new MainViewModel();
            settingsViewModel = new SettingsViewModel();
        }

        public static ViewModelBase GetViewModelInstance<T>()
        {
            if (typeof(T) == typeof(MainViewModel))
                return mainViewModel;
            else if (typeof(T) == typeof(SettingsViewModel))
                return settingsViewModel;
            return default;
        }
    }
}
