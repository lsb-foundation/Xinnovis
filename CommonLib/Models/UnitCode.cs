using System.Collections.Generic;
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
                var unitConfiguration = NameCodeConfigurationSection.GetConfig("UnitConfiguration");
                var unitCodes = new List<UnitCode>();
                foreach (NameCodeConfigurationElement element in unitConfiguration.Collection)
                {
                    unitCodes.Add(new UnitCode
                    {
                        Unit = element.Name,
                        Code = element.Code
                    });
                }
                return unitCodes;
            }
            catch { return null; }
        }
    }
}
