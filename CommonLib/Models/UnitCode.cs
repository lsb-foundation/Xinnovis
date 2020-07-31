using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Models
{
    /// <summary>
    /// 单位代码
    /// </summary>
    public class UnitCode
    {
        public string Unit { get; set; }
        public int Code { get; set; }

        public static List<UnitCode> GetUnitCodes()
        {
            return new List<UnitCode>()
            {
                new UnitCode(){ Unit = "SCCM", Code = 10 },
                new UnitCode(){ Unit = "UCCM", Code = 11 },
                new UnitCode(){ Unit = "CCM", Code = 12 },
                new UnitCode(){ Unit = "SLM", Code = 100 }
            };
        }
    }
}
