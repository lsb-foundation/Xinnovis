using System;
using System.Reflection;
using CommonLib.Extensions;
using Serilog.Core;
using MFCSoftware.Models;
using Serilog;

namespace MFCSoftware.Common
{
    public static class LogHelper
    {
        private static readonly Logger _logger;

        static LogHelper()
        {
            _logger = new LoggerConfiguration().WriteTo.File(@"Logs\.txt", rollingInterval: RollingInterval.Day).CreateLogger();
        }

        public static void WriteLog(string info)
        {
            _logger.Information(info);
        }

        public static void WriteLog(string info, Exception ex)
        {
            _logger.Error(ex, info);
        }

        public static void WriteRecievedHexData(byte[] data, SerialCommandType type)
        {
            var resolveActionAttr = type.GetType().GetField(type.ToString()).GetCustomAttribute<ResolveActionAttribute>();
            if (resolveActionAttr == null)
                throw new NullReferenceException("Can't find " + nameof(ResolveActionAttribute));
            var actionName = resolveActionAttr.ActionName;
            _logger.Information($"[Recieved] {actionName}");
            _logger.Information($"[Data] {data.ToHexString()}");
        }
    }
}
