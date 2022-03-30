using AutoCommander.Handlers.AutoCalibration.Extensions;

namespace AutoCommander.Handlers.AutoCalibration.Models;

public class DataContainer
{
    public string SerialNumber { get; private set; }
    public float Flow { get; private set; }
    public float Temperature { get; private set; }
    public float Volt { get; private set; }
    public int TimespanSeconds { get; private set; }
    public ContainerValueType ValidValue { get; private set; } = ContainerValueType.Others;
    public TemperatureType TemperatureType { get; set; }

    public void SetValue(CalibrationValueType type, string textValue)
    {
        switch (type)
        {
            case CalibrationValueType.SerialNumber:
                SerialNumber = textValue;
                break;
            case CalibrationValueType.Flow:
                Flow = float.Parse(textValue);
                break;
            case CalibrationValueType.Volt:
                ValidValue = ContainerValueType.Volt;
                Volt = float.Parse(textValue);
                break;
            case CalibrationValueType.Temperature:
                Temperature = float.Parse(textValue);
                break;
            case CalibrationValueType.Timespan:
                TimespanSeconds = int.Parse(textValue);
                break;
        }
    }

    public static DataContainer Parse(string line, TemperatureType temperatureType)
    {
        var holder = new DataContainer { TemperatureType = temperatureType };
        foreach (string kvPair in line.Split(';'))
        {
            string[] kv = kvPair.Split(':');
            if (kv.Length != 2) continue;
            holder.SetValue(kv[0].CalibrationValueType(), kv[1]);
        }
        return holder;
    }
}
