using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Extensions;

namespace CalibrationTool.Utils
{
    public static class ConfigManager
    {
        private static Configuration _config;

        private static void Write(object value, [CallerMemberName] string key = null)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            _config.SetAppSettings(key, value.ToString());
            _config.Save();
        }

        /// <summary>
        /// 从配置文件中读取指令，如果不存在，返回设定的默认值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string Read([CallerMemberName]string key = null)
        {
            if (_config.AppSettings.Settings.AllKeys.Contains(key))
            {
                string value = _config.AppSettings.Settings[key].Value;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            PropertyInfo pInfo = typeof(ConfigManager).GetProperty(key);
            if(pInfo != null)
            {
                DefaultValueAttribute defaultAttr = pInfo.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultAttr != null)
                {
                    return defaultAttr.Value.ToString();
                }
            }

            return string.Empty;
        }

        private static void Initialize()
        {
            foreach(var info in typeof(ConfigManager).GetProperties())
            {
                if (_config.AppSettings.Settings.AllKeys.Contains(info.Name))
                    continue;

                object defaultValue = info.GetCustomAttribute<DefaultValueAttribute>()?.Value;
                _config.SetAppSettings(info.Name, defaultValue?.ToString());
            }
            _config.Save();
        }

        static ConfigManager()
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Initialize();
        }

        #region 串口配置项
        public static string Serial_PortName
        {
            get => Read();
            set => Write(value);
        }

        [DefaultValue(115200)]
        public static int Serial_BaudRate
        {
            get => int.Parse(Read());
            set => Write(value);
        }
        #endregion

        #region 界面配置项
        [DefaultValue("MFM标定工具")]
        public static string AppTitle { get => Read(); }
        #endregion

        #region 指令配置项
        [DefaultValue("CALI_FLOW_V!")]
        public static string CaliFlowVCommand { get => Read(); }         //标定流量电压的指令

        [DefaultValue("CALI_VOLT")]
        public static string CaliVoltCommandHeader { get => Read(); }   //标定电压的指令头部

        [DefaultValue("CALI_K")]
        public static string CaliKCommandHeader { get => Read(); }      //标定斜率的指令的头部

        [DefaultValue("CALI_T")]
        public static string CaliTCommandHeader { get => Read(); }      //标定温度的指令的头部

        [DefaultValue("DEBUG!")]
        public static string DebugCommand { get => Read(); }        //Debug指令

        [DefaultValue("AV_START")]
        public static string AVStartCommandHeader { get => Read(); }    //开始输出1-5V电压的指令头部

        [DefaultValue("AV_STOP!")]
        public static string AVStopCommand { get => Read(); }       //停止输出1-5V电压的指令

        [DefaultValue("CHECK")]
        public static string CheckCommandHeader { get => Read(); }  //检查实际输出电压的指令头部

        [DefaultValue("CHECK_STOP!")]
        public static string CheckStopCommand { get => Read(); }    //停止检查实际输出电压的指令

        [DefaultValue("AV_1-5")]
        public static string AVCommandHeader { get => Read(); }     //写入标定1-5V的斜率K和截距D

        [DefaultValue("90")]
        public static byte[] ReadFlowCommand { get => Read().HexStringToBytes(); }     //读流量的指令
        #endregion

        #region 其他配置项
        [DefaultValue(22)]
        public static int VoltDataLength
        {
            get => int.Parse(Read());
            set => Write(value);
        }

        [DefaultValue(21)]
        public static int KDataLength
        {
            get => int.Parse(Read());
            set => Write(value);
        }
        #endregion
    }
}
