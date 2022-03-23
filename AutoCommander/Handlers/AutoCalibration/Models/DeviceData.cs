using System.Collections.Generic;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class DeviceData
{
    public string SerialNumber { get; set; }
    public List<VoltData> FlowDatas { get; set; } = new List<VoltData>();
}
