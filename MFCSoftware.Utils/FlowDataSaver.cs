using System;
using System.Timers;

namespace MFCSoftware.Utils
{
    /// <summary>
    /// 按照一定间隔时间保存数据
    /// </summary>
    public class FlowDataSaver
    {
        private readonly Timer _timer;

        public FlowData Flow { get; set; }

        public FlowDataSaver(int interval)
        {
            _timer = new Timer(TimeSpan.FromMilliseconds(interval).TotalMilliseconds) { AutoReset = false };
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void SetInterval(int milliSeconds)
        {
            if (milliSeconds > 0)
            {
                _timer.Interval = TimeSpan.FromMilliseconds(milliSeconds).TotalMilliseconds;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Flow != null)
            {
                SqliteHelper.InsertFlowData(Flow);
            }
            _timer.Start();
        }
    }
}
