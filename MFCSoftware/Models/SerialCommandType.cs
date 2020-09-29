using System;
using System.Reflection;

namespace MFCSoftware.Models
{
    public enum SerialCommandType
    {
        [ResolveAction("获取基本信息")]
        BaseInfoData,

        [ResolveAction("读取流量")]
        ReadFlow,

        [ResolveAction("清除累积流量", true, new byte[] { 0x06, 0x02, 0x00, 0x00 })]
        ClearAccuFlowData,

        [ResolveAction("设置流量", true, new byte[] { 0x06, 0x02, 0x00, 0x00 })]
        SetFlow,

        [ResolveAction("设置阀门开度", true, new byte[] { 0x06, 0x02, 0x00, 0x03 })]
        ValveControl,

        [ResolveAction("零点校准", true, new byte[] { 0x06, 0x02, 0x00, 0x01 })]
        ZeroPointCalibration,

        [ResolveAction("恢复出厂", true, new byte[] { 0x06, 0x02, 0x00, 0x02 })]
        FactoryRecovery
    }

    /// <summary>
    /// 用于实现自动解析的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ResolveActionAttribute : Attribute
    {
        public string ActionName { get; }
        public bool AutoResolve { get; }
        public byte[] CheckBytes { get; }

        public ResolveActionAttribute(string actionName) : this(actionName, false, null) { }
        public ResolveActionAttribute(string actionName, bool autoResolve, byte[] checkBytes)
        {
            ActionName = actionName;
            AutoResolve = autoResolve;
            CheckBytes = checkBytes;
        }

        private bool Check(byte[] data)
        {
            if (!AutoResolve) return true;
            if (CheckBytes == null || CheckBytes.Length <= 0) return false;
            if (CheckBytes.Length != data.Length - 3) return false;     //不校验地址及CRC

            for (int index = 0; index < CheckBytes.Length; index++)
            {
                if (CheckBytes[index] != data[index + 1])
                    return false;
            }
            return true;
        }

        public static string CheckAutomatically(byte[] data, SerialCommandType type)
        {
            var resolveNameAttr = type.GetType().GetField(type.ToString()).GetCustomAttribute<ResolveActionAttribute>();
            if (resolveNameAttr == null)
                throw new NullReferenceException("Can't find " + nameof(ResolveActionAttribute));
            if (!resolveNameAttr.AutoResolve)
                throw new ArgumentException("AutoResolve is false.", nameof(AutoResolve));
            return resolveNameAttr.ActionName + (resolveNameAttr.Check(data) ? "成功。" : "失败。");
        }
    }
}
