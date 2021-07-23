using System.Collections.Generic;

namespace AutoCalibrationTool.Models
{
    public class FlowTemperatureVolts
    {
        /// <summary>
        /// 流量
        /// </summary>
        public float Flow { get; set; }
        public float Temperature { get; set; }
        public List<float> Volts { get; set; } = new List<float>();

        public void SetValue(ValueType type, float value)
        {
            if (type == ValueType.Volt)
            {
                this.Volts.Add(value);
            }
            else if (type == ValueType.Temperature)
            {
                this.Temperature = value;
            }
        }
    }
}
