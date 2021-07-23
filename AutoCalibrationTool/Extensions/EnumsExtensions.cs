using AutoCalibrationTool.Models;

namespace AutoCalibrationTool.Extensions
{
    public static class EnumsExtensions
    {
        public static TemperatureType ToTemperatureType(this string typeString)
        {
            switch (typeString)
            {
                case "HIGH": return TemperatureType.High;
                case "MID": return TemperatureType.Mid;
                case "LOW": return TemperatureType.Low;
                default: throw new System.Exception("Unknown type.");
            }
        }

        public static ValueType ToValueType(this string typeString)
        {
            switch (typeString)
            {
                case "V": return ValueType.Volt;
                case "T": return ValueType.Temperature;
                default: throw new System.Exception("Unknown type.");
            }
        }
    }
}
