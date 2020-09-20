using CommonLib.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;

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
        private CancellationTokenSource cts = new CancellationTokenSource();
        private async void ShowMessage(string message)
        {
            if (showMessageTask != null && !showMessageTask.IsCompleted)
            {
                cts.Cancel();
                await showMessageTask;
            }

            lock (cts)
            {
                cts = new CancellationTokenSource();
                showMessageTask = Task.Run(() =>
                {
                    AppMessage = message;
                    for (int i = 0; i < 20; i++)
                    {
                        if (cts.Token.IsCancellationRequested)
                            break;
                        Thread.Sleep(100);
                    }
                    AppMessage = string.Empty;
                }, cts.Token);
            }
        }

        public static void ShowAppMessage(string message)
        {
            ViewModelBase.GetViewModelInstance<MainWindowViewModel>().ShowMessage(message);
        }
    }
}
