using System;
using System.Reflection;
using CommonLib.Extensions;
using log4net;
using MFCSoftware.Models;

namespace MFCSoftware.Common
{
    public class LogHelper
    {
        private static readonly ILog logInfo = LogManager.GetLogger("loginfo");
        private static readonly ILog logError = LogManager.GetLogger("logerror");

        public static void WriteLog(string info)
        {
            if (logInfo.IsInfoEnabled)
                logInfo.Info(info);
        }

        public static void WriteLog(string info, Exception ex)
        {
            if (logError.IsErrorEnabled)
                logError.Error(info, ex);
        }

        public static void WriteRecievedHexData(byte[] data, SerialCommandType type)
        {
            if (!logInfo.IsInfoEnabled) return;
            var resolveActionAttr = type.GetType().GetField(type.ToString()).GetCustomAttribute<ResolveActionAttribute>();
            if (resolveActionAttr == null)
                throw new NullReferenceException("Can't find " + nameof(ResolveActionAttribute));
            var actionName = resolveActionAttr.ActionName;
            logInfo.Info("[Recieved] " + actionName + Environment.NewLine + data.ToHexString());
        }
    }
}
