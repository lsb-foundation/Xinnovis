using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace AwesomeCommand.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<SerialPortInstance>();
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingViewModel>();
        }

        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();
        public SettingViewModel Setting => SimpleIoc.Default.GetInstance<SettingViewModel>();

        public void Dispose()
        {
            SimpleIoc.Default.Reset();
        }
    }
}
