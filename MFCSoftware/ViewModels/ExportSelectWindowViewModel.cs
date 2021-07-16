using GalaSoft.MvvmLight;
using System;

namespace MFCSoftware.ViewModels
{
    public class ExportSelectWindowViewModel : ViewModelBase
    {
        public ExportSelectWindowViewModel()
        {
            var now = DateTime.Now;
            ToDate = new DateTime(now.Year, now.Month, now.Day);
            FromDate = ToDate.AddDays(-1);
            FromHour = now.Hour;
            ToHour = now.Hour;
        }

        private int _fromHour;
        private int _toHour;

        public int FromHour
        {
            get => _fromHour;
            set
            {
                if (value >= 0 && value < 24)
                {
                    Set(ref _fromHour, value);
                }
            }
        }

        public int ToHour
        {
            get => _toHour;
            set
            {
                if (value >= 0 && value < 24)
                {
                    Set(ref _toHour, value);
                }
            }
        }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
