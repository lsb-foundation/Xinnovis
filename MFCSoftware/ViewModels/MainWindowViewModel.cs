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

        private Task showMessageTask;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private async void ShowMessage(string message)
        {
            if (showMessageTask != null && !showMessageTask.IsCompleted)
            {
                cts.Cancel();
                await showMessageTask;
            }

            showMessageTask = Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => AppMessage = message);
                for(int i = 0; i < 20; i++)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        Application.Current.Dispatcher.Invoke(() => AppMessage = string.Empty);
                        return;
                    }
                    Thread.Sleep(100);
                }
                Application.Current.Dispatcher.Invoke(() => AppMessage = string.Empty);
            }, cts.Token);
        }

        public static void ShowAppMessage(string message)
        {
            ViewModelBase.GetViewModelInstance<MainWindowViewModel>().ShowMessage(message);
        }
    }
}
