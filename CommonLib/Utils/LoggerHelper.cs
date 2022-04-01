using Serilog;
using Serilog.Core;
using System;

namespace CommonLib.Utils
{
    public class LoggerHelper
    {
        private static readonly Logger _logger;

        static LoggerHelper()
        {
            _logger = new LoggerConfiguration().WriteTo.File(@"logs\.txt", rollingInterval: RollingInterval.Day).CreateLogger();
        }

        public static void WriteLog(string info)
        {
            _logger.Information(info);
        }

        public static void WriteLog(string info, Exception ex)
        {
            _logger.Error(ex, info);
        }

        public static void Error(Exception e) => _logger.Error(e, e.Message);
    }
}
