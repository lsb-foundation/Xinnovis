using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Extensions;

namespace CalibrationTool.Utils
{
    public static class ConfigManager
    {
        private static Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        private static string Read([CallerMemberName]string key = null)
        {
            if (_config.AppSettings.Settings.AllKeys.Contains(key))
                return _config.AppSettings.Settings[key].Value;
            return string.Empty;
        }

        private static void Write(string value, [CallerMemberName] string key = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            _config.SetAppSettings(key, value);
            _config.Save();
        }

        private static Dictionary<string,string> _defaultSettings = new Dictionary<string, string>()
        {
            { nameof(Serial_BaudRate), "115200" }
        };

        static ConfigManager()
        {
            //自动添加配置文件中不存在的项为默认值
            foreach(string key in _defaultSettings.Keys)
            {
                if (!_config.AppSettings.Settings.AllKeys.Contains(key))
                {
                    _config.AppSettings.Settings.Add(key, _defaultSettings[key]);
                }
            }
            _config.Save();
        }

        public static string Serial_PortName
        {
            get => Read();
            set => Write(value);
        }

        public static int Serial_BaudRate
        {
            get => int.Parse(Read());
            set => Write(value.ToString());
        }
    }
}
