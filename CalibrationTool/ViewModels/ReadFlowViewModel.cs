using CommonLib.Mvvm;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CalibrationTool.ViewModels
{
    public class ReadFlowViewModel : BindableBase
    {
        private int _interval;
        public int Interval
        {
            get => _interval;
            set => SetProperty(ref _interval, value);
        }

        private bool _repeat;
        public bool Repeat
        {
            get => _repeat;
            set => SetProperty(ref _repeat, value);
        }

        private CancellationTokenSource _cancel;

        public RelayCommand SendCommand { get; set; }

        public ReadFlowViewModel(Action send)
        {
            _cancel = new CancellationTokenSource();
            SendCommand = new RelayCommand(
                o => Task.Run(async () =>
                {
                    if (_repeat && _interval <= 0) return;
                    do
                    {
                        send?.Invoke();
                        await Task.Delay(_interval);
                    } while (_repeat);
                }, _cancel.Token)
            ); 
        }
    }
}
