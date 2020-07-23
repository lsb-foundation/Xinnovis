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
        private static Configuration _config;
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

        //保存默认值
        private static Dictionary<string,string> _defaultSettings = new Dictionary<string, string>()
        {
            { nameof(Serial_BaudRate), "115200" }
        };

        static ConfigManager()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //自动添加配置文件中不存在的项为默认值
            foreach (string key in _defaultSettings.Keys)
            {
                if (!_config.AppSettings.Settings.AllKeys.Contains(key))
                {
                    _config.AppSettings.Settings.Add(key, _defaultSettings[key]);
                }
            }
            _config.Save();
        }

        public static void Intialize()
        {

        }

        #region 串口配置项
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
        #endregion

        #region 指令配置项
        public static string CaliFlowVCommand { get => Read(); }         //标定流量电压的指令
        public static string CaliVoltCommandHeader { get => Read(); }   //标定电压的指令头部
        public static string CaliKCommandHeader { get => Read(); }      //标定斜率的指令的头部
        public static string CaliTCommandHeader { get => Read(); }      //标定温度的指令的头部
        public static string DebugCommand { get => Read(); }        //Debug指令
        public static string AVStartCommandHeader { get => Read(); }    //开始输出1-5V电压的指令头部
        public static string AVStopCommand { get => Read(); }       //停止输出1-5V电压的指令
        public static string CheckCommandHeader { get => Read(); }  //检查实际输出电压的指令头部
        public static string CheckStopCommand { get => Read(); }    //停止检查实际输出电压的指令
        public static string AVCommandHeader { get => Read(); }     //写入标定1-5V的斜率K和截距D
        #endregion
    }
}
