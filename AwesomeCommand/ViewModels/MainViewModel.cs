using GalaSoft.MvvmLight;

namespace AwesomeCommand.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly SerialPortInstance _instance;

        public MainViewModel(SerialPortInstance serial)
        {
            _instance = serial;
        }

        public SerialPortInstance Instance => _instance;
    }
}