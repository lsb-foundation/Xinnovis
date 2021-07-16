using GalaSoft.MvvmLight;
using System.Threading.Tasks;

namespace CalibrationTool.ViewModels
{
    public class StatusBarViewModel : ViewModelBase
    {
        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public void ShowStatus(string message)
        {
            Task.Run(async () =>
            {
                Status = message;
                await Task.Delay(2000);
                Status = string.Empty;
            });
        }
    }
}
