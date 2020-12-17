using System.Collections.Generic;
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
                var gasTypeConfiguration = NameCodeConfigurationSection.GetConfig("GasTypeConfiguration");
                var gasTypeCodes = new List<GasTypeCode>();
                foreach (NameCodeConfigurationElement element in gasTypeConfiguration.Collection)
                {
                    gasTypeCodes.Add(new GasTypeCode
                    {
                        GasName = element.Name,
                        Code = element.Code
                    });
                }
                return gasTypeCodes;
            }
            catch { return null; }
        }
    }
}
