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

using Autofac;

namespace MFCSoftware.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static readonly IContainer _container;
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        static ViewModelLocator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MainWindowViewModel>().SingleInstance().AsSelf();
            builder.RegisterType<ChannelUserControlViewModel>().InstancePerDependency().AsSelf();
            builder.RegisterType<ExportSelectWindowViewModel>().SingleInstance().AsSelf();
            builder.RegisterType<SetAddressWindowViewModel>().SingleInstance().AsSelf();
            builder.RegisterType<SetSerialPortWindowViewModel>().SingleInstance().AsSelf();
            builder.RegisterType<AddChannelWindowViewModel>().SingleInstance().AsSelf();
            _container = builder.Build();
        }

        #region view model properties
        public static MainWindowViewModel MainViewModel => _container.Resolve<MainWindowViewModel>();
        public ChannelUserControlViewModel ChannelViewModel => _container.Resolve<ChannelUserControlViewModel>();
        public static AddChannelWindowViewModel AddChannelViewModel => _container.Resolve<AddChannelWindowViewModel>();
        public static ExportSelectWindowViewModel ExportSelectViewModel => _container.Resolve<ExportSelectWindowViewModel>();
        public static SetAddressWindowViewModel SetAddressViewModel => _container.Resolve<SetAddressWindowViewModel>();
        public static SetSerialPortWindowViewModel SetSerialViewModel => _container.Resolve<SetSerialPortWindowViewModel>();
        #endregion

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            _container.Dispose();
        }
    }
}