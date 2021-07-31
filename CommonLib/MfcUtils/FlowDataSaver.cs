﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;

namespace CommonLib.MfcUtils
{
    /// <summary>
    /// 按照一定间隔时间保存数据
    /// </summary>
    public class FlowDataSaver
    {
        private int address;
        private readonly Timer _timer;
        private readonly BlockingCollection<FlowData> _flowDatas;
        public FlowData Flow { get; set; }
        public FlowDataSaver(int address, int intervalSeconds)
        {
            this.address = address;
            _flowDatas = new BlockingCollection<FlowData>();
            Task.Run(() => InsertFlowData());
            _timer = new Timer(TimeSpan.FromSeconds(intervalSeconds).TotalMilliseconds) { AutoReset = false };
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void SetAddress(int addr)
        {
            address = addr;
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
                DbStorage.InsertFlowData(address, flowData);
            }
        }
    }
}
