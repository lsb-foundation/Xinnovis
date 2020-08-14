using System.ComponentModel;

namespace SerialDataDisplay
{
    public enum SendType
    {
        [Description("风速")]
        WindSpeed,

        [Description("风速原始数据")]
        OriginDataOfWindSpeed,

        [Description("温度")]
        Temperature,

        [Description("温度原始数据")]
        OriginDataOfTemperature,

        [Description("CALI_FLOW_V!")]
        CaliFlowV
    }
}
