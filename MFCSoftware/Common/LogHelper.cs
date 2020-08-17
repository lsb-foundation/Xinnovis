using System;
using log4net;

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
    }
}
