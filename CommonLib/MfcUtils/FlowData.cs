using System;

namespace CommonLib.MfcUtils
{
    public class FlowData
    {
        public float CurrentFlow { get; set; }
        public string Unit { get; set; }
        public float AccuFlow { get; set; }
        public string AccuFlowUnit { get; set; }
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }

        public DateTime CollectTime { get; set; }   //导出数据时用到该属性
    }
}
