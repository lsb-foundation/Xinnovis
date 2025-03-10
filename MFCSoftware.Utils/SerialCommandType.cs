﻿using System;
using System.Reflection;

namespace MFCSoftware.Utils
{
    public enum SerialCommandType
    {
        [ResolveAction("读取地址", true, new byte[] { 0xFE, 0x03, 0x02, 0x00 })]
        ReadAddress,

        //[ResolveAction("设置地址", true, new byte[] { 0xFE, 0x06, 0x02, 0x00 })]
        [ResolveAction("设置地址", true, new byte[] { 0xfe, 0x06, 0x00, 0x00, 0x00 })]
        WriteAddress,

        //[ResolveAction("设置波特率", true, new byte[] { 0xFE, 0x06, 0x02, 0x00 })]
        [ResolveAction("设置波特率", true, new byte[] { 0xfe, 0x06, 0x00, 0x01, 0x00 })]
        SetBaudRate,

        [ResolveAction("获取基本信息")]
        BaseInfoData,
        
        [ResolveAction("获取版本号")]
        ReadVersion,

        [ResolveAction("读取流量")]
        ReadFlow,

        //[ResolveAction("清除累积流量", true, new byte[] { 0x06, 0x02, 0x00, 0x00 })]    //最原始的版本
        [ResolveAction("清除累积流量", true, new byte[] { 0x10, 0x00, 0x18, 0x00, 0x04 })] //按照标准modbus协议修改 2021.09.01
        ClearAccuFlowData,

        //[ResolveAction("设置流量", true, new byte[] {0x10, 0x02, 0x00, 0x00 })] //针对客户修改 2021.08.17
        [ResolveAction("设置流量", true, new byte[] { 0x10, 0x00, 0x21, 0x00, 0x03 })] //按照标准Modbus协议修改 2021.09.01
        SetFlow,

        //[ResolveAction("设置阀门开度", true, new byte[] { 0x06, 0x02, 0x00, 0x03 })]
        //[ResolveAction("设置阀门开度", true, new byte[] { 0x10, 0x02, 0x00, 0x03 })] //针对客户修改 2021.08.17
        [ResolveAction("设置阀门开度", true, new byte[] { 0x10, 0x00, 0x21, 0x00, 0x04 })]
        ValveControl,

        //[ResolveAction("零点校准", true, new byte[] { 0x06, 0x02, 0x00, 0x01 })]
        [ResolveAction("零点校准", true, new byte[] { 0x06, 0x00, 0x25, 0x00, 0x01 })]
        ZeroPointCalibration,

        //[ResolveAction("恢复出厂", true, new byte[] { 0x06, 0x02, 0x00, 0x02 })]
        [ResolveAction("恢复出厂", true, new byte[] { 0x06, 0x00, 0x25, 0x00, 0x02 })]
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

        public bool Check(byte[] data, int startIndex = 1)
        {
            if (!AutoResolve)
            {
                return true;
            }

            if (CheckBytes == null || CheckBytes.Length <= 0)
            {
                return false;
            }
            if (CheckBytes.Length > data.Length - 2 - startIndex)
            {
                return false; //校验位在数据接收后第一时间检查，这里不做处理
            }

            for (int index = 0; index < CheckBytes.Length; index++)
            {
                if (CheckBytes[index] != data[index + startIndex])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 自动检查，默认第一位为地址
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string CheckAutomatically(byte[] data, SerialCommandType type)
        {
            var resolveActionAttr = type.GetType().GetField(type.ToString()).GetCustomAttribute<ResolveActionAttribute>();
            if (resolveActionAttr == null)
            {
                throw new NullReferenceException("Can't find " + nameof(ResolveActionAttribute));
            }
            if (!resolveActionAttr.AutoResolve)
            {
                throw new ArgumentException("AutoResolve is false.", nameof(AutoResolve));
            }
            return resolveActionAttr.ActionName + (resolveActionAttr.Check(data) ? "成功。" : "失败。");
        }
    }
}
