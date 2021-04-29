using System.Collections.Generic;
using System.Linq;
using CommonLib.Config;

namespace CommonLib.Models
{
    /// <summary>
    /// 单位代码
    /// </summary>
    public class UnitCode
    {
        public string Unit { get; set; }
        public int Code { get; set; }

        public static List<UnitCode> GetUnitCodesFromConfiguration()
        {
            try
            {
                return NameCodeConfigurationSection
                    .GetConfig("UnitConfiguration")
                    .Collection
                    .OfType<NameCodeConfigurationElement>()
                    .Select(e => new UnitCode { Unit = e.Name, Code = e.Code })
                    .ToList();
            }
            catch { return null; }
        }
    }
}
