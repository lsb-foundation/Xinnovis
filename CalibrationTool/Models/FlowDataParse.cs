using CommonLib.Extensions;
using System;

namespace CalibrationTool.Models
{
    /// <summary>
    /// 解析流量数据
    /// </summary>
    public class FlowDataParse : IParse<byte[], double>
    {
        public double Resolve(byte[] data)
        {
            bool isDataCorrect = data.Length == 8 && data[6] == 0x90 && data[7] == 0xED;

            if (!isDataCorrect) throw new Exception("接收到的数据有误，无法解析！");

            int intValue = data.ToInt32(0, 2);
            return intValue / 100.0;
        }
    }
}
