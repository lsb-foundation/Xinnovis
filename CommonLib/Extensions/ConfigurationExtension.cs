using System.Configuration;
using System.Linq;

namespace CommonLib.Extensions
{
    /// <summary>
    /// Configuration类的扩展
    /// </summary>
    public static class ConfigurationExtension
    {
        /// <summary>
        /// 用于设置AppSettings，改变已有设置项或者添加新设置项。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetAppSettings(this Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings.AllKeys.Contains(key))
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }
        }
    }
}
