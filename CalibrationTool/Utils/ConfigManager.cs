using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                    return defaultAttr.Value.ToString();
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
        /// <summary>
        /// 保存选择的串口号。
        /// </summary>
        public static string Serial_PortName
        {
            get => Read();
            set => Write(value);
        }

        /// <summary>
        /// 保存选择的波特率。
        /// </summary>
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
        /// <summary>
        /// 标定流量电压的指令。
        /// </summary>
        [DefaultValue("CALI_FLOW_V!")]
        public static string CaliFlowVCommand { get => Read(); }

        /// <summary>
        /// 标定电压的指令的头部。
        /// </summary>
        [DefaultValue("CALI_VOLT")]
        public static string CaliVoltCommandHeader { get => Read(); }

        /// <summary>
        /// 标定斜率的指令的头部。
        /// </summary>
        [DefaultValue("CALI_K")]
        public static string CaliKCommandHeader { get => Read(); }

        /// <summary>
        /// 标定温度的指令的头部。
        /// </summary>
        [DefaultValue("CALI_T")]
        public static string CaliTCommandHeader { get => Read(); }

        /// <summary>
        /// Debug指令。
        /// </summary>
        [DefaultValue("DEBUG!")]
        public static string DebugCommand { get => Read(); }

        /// <summary>
        /// 开始输出1-5V电压的指令的头部。
        /// </summary>
        [DefaultValue("AV_START")]
        public static string AVStartCommandHeader { get => Read(); }

        /// <summary>
        /// 停止输出1-5V电压的指令。
        /// </summary>
        [DefaultValue("AV_STOP!")]
        public static string AVStopCommand { get => Read(); }

        /// <summary>
        /// 开始输出4-20mA电流的指令头部
        /// </summary>

        [DefaultValue("AI_START")]
        public static string AIStartCommandHeader { get => Read(); }

        /// <summary>
        /// 停止输出4-20mA电流的指令
        /// </summary>
        [DefaultValue("AI_STOP!")]
        public static string AIStopCommand { get => Read(); }

        /// <summary>
        /// 检查实际输出电压的指令的头部。
        /// </summary>
        [DefaultValue("CHECK_1-5")]
        public static string CheckAVStartCommandHeader { get => Read(); }

        /// <summary>
        /// 停止检查的指令。
        /// </summary>
        [DefaultValue("CHECK_STOP!")]
        public static string CheckStopCommand { get => Read(); }

        /// <summary>
        /// 检查实际输出的电流的指令的头部。
        /// </summary>
        [DefaultValue("CHECK_4-20")]
        public static string CheckAIStartCommandHeader { get => Read(); }

        /// <summary>
        /// 写入标定1-5V的斜率K和截距D的指令的头部。
        /// </summary>
        [DefaultValue("AV_1-5")]
        public static string AVCommandHeader { get => Read(); } 

        /// <summary>
        /// 写入标定4-20mA的斜率K和截距D的指令的头部。
        /// </summary>

        [DefaultValue("AI_4-20")]
        public static string AICommandHeader { get => Read(); }

        /// <summary>
        /// 清除寄存器指令。
        /// </summary>
        [DefaultValue("CLEAR_EEPROM!")]
        public static string ClearEEPRomCommand { get => Read(); }

        /// <summary>
        /// PWM测试开始指令。
        /// </summary>
        [DefaultValue("PWM_TEST_START!")]
        public static string PWMTestStartCommand { get => Read(); }

        /// <summary>
        /// PWM测试停止指令。
        /// </summary>
        [DefaultValue("PWM_TEST_STOP!")]
        public static string PWMTestStopCommand { get => Read(); }

        /// <summary>
        /// PID数值设定指令的头部。
        /// </summary>
        [DefaultValue("PWM_VALUE")]
        public static string PWMCommandHeader { get => Read(); }

        /// <summary>
        /// 写入气体信息的指令的头部。
        /// </summary>
        [DefaultValue("GAS")]
        public static string GasCommandHeader { get => Read(); }

        /// <summary>
        /// 气体系数设定指令的头部。
        /// </summary>
        [DefaultValue("GAS_FACTOR")]
        public static string GasFactorCommandHeader { get => Read(); }

        /// <summary>
        /// 读流量指令
        /// </summary>
        [DefaultValue("90")]
        public static byte[] ReadFlowCommand { get => Read().HexStringToBytes(); }
        #endregion

        #region 其他配置项
        /// <summary>
        /// 写入标定电压的数据长度配置项。
        /// </summary>
        [DefaultValue(22)]
        public static int VoltDataLength
        {
            get => int.Parse(Read());
            set => Write(value);
        }

        /// <summary>
        /// 写入标定斜率的数据长度配置项。
        /// </summary>
        [DefaultValue(21)]
        public static int KDataLength
        {
            get => int.Parse(Read());
            set => Write(value);
        }
        #endregion
    }
}
