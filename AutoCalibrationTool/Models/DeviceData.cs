using System.Collections.Generic;

namespace AutoCalibrationTool.Models
{
    public class DeviceData
    {
        public int DeviceCode { get; set; }
        public List<FlowTemperatureData> Datas { get; set; }
    }
}
