namespace AutoCommander.Handlers.AutoCalibration.Extensions;

public static class EnumsExtensions
{
    public static Models.ValueType ToValueType(this string typeString) =>
        typeString switch
        {
            "V" => Models.ValueType.Volt,
            "T" => Models.ValueType.Temperature,
            "A" => Models.ValueType.Timespan,
            _ => Models.ValueType.SerialNumber
        };
}
