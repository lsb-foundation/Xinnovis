using System.Collections.Generic;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class CalibrationVolts
{
    public float Flow { get; set; }
    public float Temperature { get; set; }
    public int TimespanSeconds { get; set; }
    public List<float> Volts { get; set; } = new();

    public void Set(DataContainer dataHolder)
    {
        Flow = dataHolder.Flow;
        if (dataHolder.ValidValue == ContainerValueType.Volt)
        {
            Volts.Add(dataHolder.Volt);
        }
        else
        {
            Temperature = dataHolder.Temperature;
            TimespanSeconds = dataHolder.TimespanSeconds;
        }
    }
}
