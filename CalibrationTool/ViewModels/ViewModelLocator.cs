/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:CalibrationTool"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;

namespace CalibrationTool.ViewModels
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
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<ReadDataViewModel>();
            SimpleIoc.Default.Register<SerialPortViewModel>();
            SimpleIoc.Default.Register<StatusBarViewModel>();
            SimpleIoc.Default.Register<WriteDataViewModel>();
        }

        public static IServiceLocator Locator => ServiceLocator.Current;

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            SimpleIoc.Default.Reset();
        }
    }
}