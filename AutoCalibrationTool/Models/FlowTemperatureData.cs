using System.Collections.Generic;

namespace AutoCalibrationTool.Models
{
    public class FlowTemperatureData
    {
        /// <summary>
        /// 流量
        /// </summary>
        public float Flow { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public float Temperature { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public List<float> Volts { get; set; }
    }
}
