using System.Collections.Generic;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class VoltData
{
    /// <summary>
    /// 流量
    /// </summary>
    public float Flow { get; set; }
    public float Temperature { get; set; }
    public int TimespanSeconds { get; set; }
    public List<float> Volts { get; set; } = new();
}
