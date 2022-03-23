using System.Reflection;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace MultipleDevicesMonitor.ViewModels
{
    public class AboutViewModel : ObservableObject
    {
        public string Version 
        {
            get => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
