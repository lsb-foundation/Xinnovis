using CommonLib.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace MFCSoftware.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
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
                RaisePropertyChanged(nameof(Enable));
            }
        }

        public string AppName
        {
            get
            {
                var version = App.ResourceAssembly.GetName().Version;
                string appVersion = $" v{version}";
                return ConfigurationManager.AppSettings["AppName"] + appVersion;
            }
        }

        private readonly bool _readTemperature = ConfigurationManager.AppSettings["ReadTemperature"].Trim().ToLower() == "true";
        public bool ReadTemperature => _readTemperature;

        private string _appMessage;
        public string AppMessage
        {
            get => _appMessage;
            set => Set(ref _appMessage, value);
        }

        private Task showMessageTask;
        private readonly object syncObject = new object();
        private CancellationTokenSource cts;
        public async void ShowMessage(string message)
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
                    try
                    {
                        App.Current.Dispatcher.Invoke(() => AppMessage = message);
                        for (int i = 0; i < 20; i++)
                        {
                            if (cts.Token.IsCancellationRequested)
                                break;
                            Thread.Sleep(100);
                        }
                        App.Current.Dispatcher.Invoke(() => AppMessage = string.Empty);
                    }
                    catch (Exception e)
                    {
                        LoggerHelper.WriteLog(e.Message, e);
                        throw e;
                    }
                }, cts.Token);
            }
        }
    }
}
