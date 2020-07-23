
using System;

namespace CalibrationTool.Models
{
    public class DebugData
    {
        [DebugDataMapper("SN")]
        public string SN { get; set; }

        [DebugDataMapper("Type of GAS")]
        public string GasType { get; set; }

        [DebugDataMapper("Range")]
        public string Range { get; set; }

        [DebugDataMapper("UNIT")]
        public string Unit { get; set; }

        [DebugDataMapper("modbus addr")]
        public string ModbusAddr { get; set; }

        [DebugDataMapper("modbus baud")]
        public string ModbusBaud { get; set; }

        [DebugDataMapper("K of 1-5V")]
        public string KOf1_5 { get; set; }

        [DebugDataMapper("D of 1-5V")]
        public string DOf1_5 { get; set; }

        [DebugDataMapper("T of cali flow")]
        public string TOfCaliFlow { get; set; }
    }

    public class DebugDataMapperAttribute : Attribute
    {
        public string MappedValue { get; private set; }
        public DebugDataMapperAttribute(string mappedValue)
        {
            this.MappedValue = mappedValue;
        }
    }
}
