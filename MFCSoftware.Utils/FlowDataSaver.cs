using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;

namespace MFCSoftware.Utils
{
    /// <summary>
    /// 按照一定间隔时间保存数据
    /// </summary>
    public class FlowDataSaver
    {
        private readonly Timer _timer;
        private readonly BlockingCollection<FlowData> _flowDatas;
        public FlowData Flow { get; set; }
        public FlowDataSaver(int intervalSeconds)
        {
            _flowDatas = new BlockingCollection<FlowData>();
            _ = Task.Run(() => InsertFlowData());
            _timer = new Timer(TimeSpan.FromSeconds(intervalSeconds).TotalMilliseconds) { AutoReset = false };
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void SetInterval(int seconds)
        {
            if (seconds > 0)
            {
                _timer.Interval = TimeSpan.FromSeconds(seconds).TotalMilliseconds;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Flow != null)
            {
                _flowDatas.Add(Flow);
            }
            _timer.Start();
        }

        private void InsertFlowData()
        {
            foreach (var flowData in _flowDatas.GetConsumingEnumerable())
            {
                SqliteHelper.InsertFlowData(flowData);
            }
        }
    }
}
