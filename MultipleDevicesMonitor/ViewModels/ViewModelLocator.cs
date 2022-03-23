/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:MultipleDevicesMonitor"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace MultipleDevicesMonitor.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            var services = new ServiceCollection()
                .AddSingleton<MainViewModel>()
                .AddSingleton<SerialViewModel>()
                .AddSingleton<SettingsViewModel>()
                .AddSingleton<AboutViewModel>();
            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        public MainViewModel Main => Ioc.Default.GetService<MainViewModel>();
        public SerialViewModel Serial => Ioc.Default.GetService<SerialViewModel>();
        public SettingsViewModel Settings => Ioc.Default.GetService<SettingsViewModel>();
        public AboutViewModel About => Ioc.Default.GetService<AboutViewModel>();
    }
}