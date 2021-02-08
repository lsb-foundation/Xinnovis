using CommonLib.Mvvm;
using System.Threading.Tasks;

namespace CalibrationTool.ViewModels
{
    public class StatusBarViewModel : ViewModelBase
    {
        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public void ShowStatus(string message)
        {
            Task.Run(async () =>
            {
                _status = message;
                RaiseProperty(nameof(Status));
                await Task.Delay(2000);
                _status = string.Empty;
                RaiseProperty(nameof(Status));
            });
        }
    }
}
