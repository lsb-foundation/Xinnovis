using CommonLib.Utils;
using GalaSoft.MvvmLight;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

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

        private CancellationTokenSource cts;
        public async void ShowMessage(string message)
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                await App.Current.Dispatcher.InvokeAsync(() => AppMessage = message, DispatcherPriority.Normal, cts.Token);
                await Task.Delay(2000, cts.Token);
                await App.Current.Dispatcher.InvokeAsync(() => AppMessage = string.Empty, DispatcherPriority.Normal, cts.Token);
            }
            catch (TaskCanceledException) { }
        }
    }
}
