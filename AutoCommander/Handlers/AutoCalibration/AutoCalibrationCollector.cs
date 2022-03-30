using AutoCommander.Handlers.AutoCalibration.Models;
using CommonLib.Utils;
using System;

namespace AutoCommander.Handlers.AutoCalibration;

public class AutoCalibrationCollector
{
    private readonly Collector<DataContainer> _collector;

    public event Action Completed;
    public CollectionContainer Container { get; }

    public AutoCalibrationCollector()
    {
        Container = new();
        _collector = new()
        {
            Splitter = "\r\n",
            Parser = ParseText,
            Handler = data => Container.Insert(data)
        };
    }

    public void Insert(string text) => _collector.Insert(text);

    private DataContainer ParseText(string line) =>
        line switch
        {
            string ln when ln.StartsWith("HIGH") => DataContainer.Parse(ln, TemperatureType.High),
            string ln when ln.StartsWith("MID") => DataContainer.Parse(ln, TemperatureType.Mid),
            string ln when ln.StartsWith("LOW") => DataContainer.Parse(ln, TemperatureType.Low),
            string ln when ln.StartsWith("CALIBRATION_OVER!") => CalibrationOver(),
            _ => null
        };

    private DataContainer CalibrationOver()
    {
        LoggerHelper.WriteLog("自动标定结束！");
        Completed?.Invoke();
        return null;
    }
}
