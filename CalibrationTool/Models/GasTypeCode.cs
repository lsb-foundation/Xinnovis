using System.Collections.Generic;

namespace CalibrationTool.Models
{
    /// <summary>
    /// 气体类型代码
    /// </summary>
    public class GasTypeCode
    {
        public string GasName { get; set; }
        public int Code { get; set; }

        public static List<GasTypeCode> GetGasTypeCodes()
        {
            return new List<GasTypeCode>()
            {
                new GasTypeCode(){ GasName="空气(Air)", Code = 8},
                new GasTypeCode(){ GasName="氩气(Ar)", Code = 4},
                new GasTypeCode(){ GasName="二氧化碳(CO2)", Code = 25},
                new GasTypeCode(){ GasName="氦气(He)", Code = 1},
                new GasTypeCode(){ GasName="氢气(H2)", Code = 7},
                new GasTypeCode(){ GasName="甲烷(CH4)", Code = 28},
                new GasTypeCode(){ GasName="氮气(N2)", Code = 13},
                new GasTypeCode(){ GasName="氧气(O2)", Code = 15}
            };
        }
    }
}
