namespace AutoCommander.Handlers.AutoCalibration.Models;

public class CalibrationDataHolder
{
    public string SerialNumber { get; set; }
    public float Flow { get; set; }
    public float Temperature { get; set; }
    public float Volt { get; set; }
    public int TimespanSeconds { get; set; }

    public void SetValueForType(CalibrationValueType type, string textValue)
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
}
