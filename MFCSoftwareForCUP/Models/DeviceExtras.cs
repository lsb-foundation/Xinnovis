using GalaSoft.MvvmLight;

namespace MFCSoftwareForCUP.Models
{
    public class DeviceExtras : ObservableObject
    {
        public int Address { get; set; }
        public string Floor { get; set; }
        public string Room { get; set; }
        public string GasType { get; set; }
    }
}
