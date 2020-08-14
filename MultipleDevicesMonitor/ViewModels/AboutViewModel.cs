using System.Reflection;
using CommonLib.Mvvm;

namespace MultipleDevicesMonitor.ViewModels
{
    public class AboutViewModel:ViewModelBase
    {
        public string Version 
        {
            get => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
