﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommonLib.Communication.Serial;

namespace CalibrationTool.Models
{
    public class DebugData
    {
        [DebugDataMapper("SN")]
        public string SN { get; set; }

        [DebugDataMapper("Type of GAS")]
        public string GasType { get; set; }

        [DebugDataMapper("Range")]
        public string Range { get; set; }

        [DebugDataMapper("UNIT")]
        public string Unit { get; set; }

        [DebugDataMapper("modbus addr")]
        public string ModbusAddr { get; set; }

        private string _modbusBaud;

        [DebugDataMapper("modbus baud")]
        [InvokeCustomSetMethod]
        public string ModbusBaud
        {
            get => _modbusBaud;
            set
            {
                if (BaudRateCode.GetBaudRateCodes().FirstOrDefault(v => v.Code.ToString() == value) is BaudRateCode code)
                {
                    _modbusBaud = code.BaudRate.ToString();
                }
            }
        }

        [DebugDataMapper("K of 4-20mA")]
        public string KOf4_20mA { get; set; }

        [DebugDataMapper("D of 4-20mA")]
        public string DOf4_20mA { get; set; }

        [DebugDataMapper("K of 1-5V")]
        public string KOf1_5V { get; set; }

        [DebugDataMapper("D of 1-5V")]
        public string DOf1_5V { get; set; }

        [DebugDataMapper("T of cali flow")]
        public string TOfCaliFlow { get; set; }

        [DebugDataMapper("T of now")]
        public string TOfNow { get; set; }

        [DebugDataMapper("GAS FACTOR")]
        public string GasFactor { get; set; }

        #region PID参数
        [DebugDataMapper("P")]
        public string P { get; set; }
        [DebugDataMapper("I")]
        public string I { get; set; }
        [DebugDataMapper("D")]
        public string D { get; set; }
        [DebugDataMapper("Zero")]
        public string Zero { get; set; }
        #endregion

        public bool TryToSetPropertyValue(PropertyInfo pInfo, string value)
        {
            try
            {
                if (pInfo.GetCustomAttribute<InvokeCustomSetMethodAttribute>() is null)
                    pInfo.SetValue(this, value);
                else
                    pInfo.SetMethod?.Invoke(this, new object[] { value });
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class DebugDataMapperAttribute : Attribute
    {
        public string MappedKey { get; private set; }
        public DebugDataMapperAttribute(string mappedKey)
        {
            this.MappedKey = mappedKey;
        }

        private static Dictionary<string, PropertyInfo> keyToPropertyMap;

        static DebugDataMapperAttribute()
        {
            keyToPropertyMap = new Dictionary<string, PropertyInfo>();
            foreach(var property in typeof(DebugData).GetProperties())
            {
                string key = property.GetCustomAttribute<DebugDataMapperAttribute>()?.MappedKey;
                if (string.IsNullOrEmpty(key)) continue;
                if (keyToPropertyMap.Keys.Contains(key)) continue;
                keyToPropertyMap.Add(key, property);
            }
        }

        public static PropertyInfo GetPropertyByKey(string key)
        {
            if (!keyToPropertyMap.Keys.Contains(key)) 
                return null;
            return keyToPropertyMap[key];
        }
    }

    /// <summary>
    /// 表示该属性需要调用自定义的set方法来设置其值。
    /// </summary>
    public sealed class InvokeCustomSetMethodAttribute : Attribute { }
}
