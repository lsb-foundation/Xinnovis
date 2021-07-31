using System;
using System.Reflection;
using CommonLib.Extensions;
using CommonLib.MfcUtils;
using CommonLib.Utils;

namespace MFCSoftware.Common
{
    public static class LogHelper
    {
        public static void WriteRecievedHexData(byte[] data, SerialCommandType type)
        {
            var resolveActionAttr = type.GetType().GetField(type.ToString()).GetCustomAttribute<ResolveActionAttribute>();
            if (resolveActionAttr == null)
                throw new NullReferenceException("Can't find " + nameof(ResolveActionAttribute));
            var actionName = resolveActionAttr.ActionName;
            LoggerHelper.WriteLog($"[Recieved] {actionName}");
            LoggerHelper.WriteLog($"[Data] {data.ToHexString()}");
        }
    }
}
