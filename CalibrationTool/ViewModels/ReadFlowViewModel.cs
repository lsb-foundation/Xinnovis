using CommonLib.Mvvm;
using System;

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
            set
            {
                SetProperty(ref _repeat, value);
            }
        }

        public RelayCommand SendCommand { get; set; }

        public ReadFlowViewModel()
        {
            
        }

        private void SendOnce()
        {

        }

        private void SendRepeat()
        {
            
        }
    }
}
