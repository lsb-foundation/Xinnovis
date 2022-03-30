using AutoCommander.Handlers.AutoCalibration.Models;

namespace AutoCommander.Handlers.AutoCalibration.Extensions;

public static class EnumsExtensions
{
    public static CalibrationValueType CalibrationValueType(this string typeString) =>
        typeString switch
        {
            "V" => Models.CalibrationValueType.Volt,
            "T" => Models.CalibrationValueType.Temperature,
            "A" => Models.CalibrationValueType.Timespan,
            "F" => Models.CalibrationValueType.Flow,
            _ => Models.CalibrationValueType.SerialNumber
        };
}
