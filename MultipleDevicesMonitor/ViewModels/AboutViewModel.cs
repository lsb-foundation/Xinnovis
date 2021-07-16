using System.Reflection;
using GalaSoft.MvvmLight;

namespace MultipleDevicesMonitor.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public string Version 
        {
            get => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
