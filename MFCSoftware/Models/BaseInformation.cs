using CommonLib.Models;

namespace MFCSoftware.Models
{
    public class BaseInformation
    {
        public string SN { get; set; }
        public int Range { get; set; }
        public GasTypeCode GasType { get; set; }
        public UnitCode Unit { get; set; }
    }
}
