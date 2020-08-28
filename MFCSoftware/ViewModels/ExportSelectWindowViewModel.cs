using CommonLib.Mvvm;
using System;

namespace MFCSoftware.ViewModels
{
    public class ExportSelectWindowViewModel:BindableBase
    {
        public ExportSelectWindowViewModel()
        {
            var now = DateTime.Now;
            ToDate = new DateTime(now.Year, now.Month, now.Day);
            FromDate = ToDate.AddDays(-1);
            FromHour = now.Hour;
            ToHour = now.Hour;
        }

        public int FromHour { get; set; }
        public int ToHour { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
