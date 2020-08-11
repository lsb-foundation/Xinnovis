using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultipleDevicesMonitor.ViewModels
{
    public class ViewModelManager
    {
        private static MainViewModel mainViewModel;

        static ViewModelManager()
        {
            mainViewModel = new MainViewModel();
        }

        public static T GetViewModelInstance<T>() where T : class
        {
            if (typeof(T) == typeof(MainViewModel))
                return mainViewModel as T;
            return default;
        }
    }
}
