namespace AutoCommander.Handlers.AutoCalibration.Extensions;

public static class EnumsExtensions
{
    public static Models.CalibrationValueType CalibrationValueType(this string typeString) =>
        typeString switch
        {
            "V" => Models.CalibrationValueType.Volt,
            "T" => Models.CalibrationValueType.Temperature,
            "A" => Models.CalibrationValueType.Timespan,
            _ => Models.CalibrationValueType.SerialNumber
        };
}
