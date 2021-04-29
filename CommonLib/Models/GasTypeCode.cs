using System.Collections.Generic;
using System.Linq;
using CommonLib.Config;

namespace CommonLib.Models
{
    /// <summary>
    /// 气体类型代码
    /// </summary>
    public class GasTypeCode
    {
        public string GasName { get; set; }
        public int Code { get; set; }

        public static List<GasTypeCode> GetGasTypeCodesFromConfiguration()
        {
            try
            {
                return NameCodeConfigurationSection
                    .GetConfig("GasTypeConfiguration")
                    .Collection
                    .OfType<NameCodeConfigurationElement>()
                    .Select(e => new GasTypeCode { GasName = e.Name, Code = e.Code })
                    .ToList();

            }
            catch { return null; }
        }
    }
}
