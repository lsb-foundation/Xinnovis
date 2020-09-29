using CommonLib.Mvvm;
using System;
using System.Configuration;
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

        public string AppName
        {
            get => ConfigurationManager.AppSettings["AppName"] + "-青岛芯笙";
        }

        private string _appMessage;
        public string AppMessage
        {
            get => _appMessage;
            set => SetProperty(ref _appMessage, value);
        }

        private Task showMessageTask;
        private readonly object syncObject = new object();
        private CancellationTokenSource cts;
        private async void ShowMessage(string message)
        {
            if (showMessageTask != null && !showMessageTask.IsCompleted)
            {
                cts?.Cancel();
                await showMessageTask;
            }

            lock (syncObject)
            {
                cts = new CancellationTokenSource();
                showMessageTask = Task.Run(() =>
                {
                    App.Current.Dispatcher.Invoke(() => AppMessage = message);
                    for (int i = 0; i < 20; i++)
                    {
                        if (cts.Token.IsCancellationRequested)
                            break;
                        Thread.Sleep(100);
                    }
                    App.Current.Dispatcher.Invoke(() => AppMessage = string.Empty);
                }, cts.Token);
            }
        }

        public static void ShowAppMessage(string message)
        {
            ViewModelBase.GetViewModelInstance<MainWindowViewModel>().ShowMessage(message);
        }
    }
}
