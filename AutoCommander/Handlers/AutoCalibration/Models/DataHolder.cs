namespace AutoCommander.Handlers.AutoCalibration.Models;

public class DataHolder
{
    public string Device { get; set; }
    public float Flow { get; set; }
    public float Temperature { get; set; }
    public float Volt { get; set; }
    public int TimespanSeconds { get; set; }

    public void SetValueForType(ValueType type, string textValue)
    {
        switch (type)
        {
            case ValueType.SerialNumber:
                Device = textValue;
                break;
            case ValueType.Flow:
                Flow = float.Parse(textValue);
                break;
            case ValueType.Volt:
                Volt = float.Parse(textValue);
                break;
            case ValueType.Temperature:
                Temperature = float.Parse(textValue);
                break;
            case ValueType.Timespan:
                TimespanSeconds = int.Parse(textValue);
                break;
        }
    }
}
