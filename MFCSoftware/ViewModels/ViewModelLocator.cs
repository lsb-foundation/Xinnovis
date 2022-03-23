/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:MFCSoftware"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace MFCSoftware.ViewModels
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
        static ViewModelLocator()
        {
            var services = new ServiceCollection()
                .AddSingleton<MainWindowViewModel>()
                .AddTransient<ChannelUserControlViewModel>()
                .AddSingleton<ExportSelectWindowViewModel>()
                .AddSingleton<SetAddressWindowViewModel>()
                .AddSingleton<SetSerialPortWindowViewModel>()
                .AddSingleton<AddChannelWindowViewModel>();

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        #region view model properties
        public static MainWindowViewModel MainViewModel => Ioc.Default.GetService<MainWindowViewModel>();
        public ChannelUserControlViewModel ChannelViewModel => Ioc.Default.GetService<ChannelUserControlViewModel>();
        public static AddChannelWindowViewModel AddChannelViewModel => Ioc.Default.GetService<AddChannelWindowViewModel>();
        public static ExportSelectWindowViewModel ExportSelectViewModel => Ioc.Default.GetService<ExportSelectWindowViewModel>();
        public static SetAddressWindowViewModel SetAddressViewModel => Ioc.Default.GetService<SetAddressWindowViewModel>();
        public static SetSerialPortWindowViewModel SetSerialViewModel => Ioc.Default.GetService<SetSerialPortWindowViewModel>();
        #endregion
    }
}