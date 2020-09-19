using CommonLib.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MFCSoftware.ViewModels
{
    public class MainWindowViewModel:ViewModelBase
    {
        public bool Enable
        {
            get => _channelCount <= 0;
        }

        private int _channelCount;
        public int ChannelCount
        {
            get => _channelCount;
            set
            {
                _channelCount = value;
                RaiseProperty(nameof(Enable));
            }
        }

        private string _appMessage;
        public string AppMessage
        {
            get => _appMessage;
            set => SetProperty(ref _appMessage, value);
        }

        private void ShowMessage(string message)
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => AppMessage = message);
                Thread.Sleep(2000);
                Application.Current.Dispatcher.Invoke(() => AppMessage = string.Empty);
            });
        }

        public static void ShowAppMessage(string message)
        {
            ViewModelBase.GetViewModelInstance<MainWindowViewModel>().ShowMessage(message);
        }
    }
}
