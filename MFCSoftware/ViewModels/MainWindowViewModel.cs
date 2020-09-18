using CommonLib.Mvvm;
using System.Threading.Tasks;

namespace MFCSoftware.ViewModels
{
    public class MainWindowViewModel:BindableBase
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
        public async void ShowMessage(string message)
        {
            if (showMessageTask != null)
                await showMessageTask;

            showMessageTask = Task.Run(async () =>
            {
                AppMessage = message;
                await Task.Delay(2000);
                AppMessage = string.Empty;
            });
        }
    }
}
