using System.Collections.Generic;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class CalibrationData
{
    public string SerialNumber { get; set; }
    public List<CalibrationVoltData> FlowDatas { get; set; } = new List<CalibrationVoltData>();
}
