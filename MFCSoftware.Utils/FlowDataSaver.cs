using CommonLib.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MFCSoftware.Utils
{
    /// <summary>
    /// 按照一定间隔时间保存数据
    /// </summary>
    public class FlowDataSaver
    {
        private DateTime _lastInsertTime = DateTime.Now;

        public uint Interval { get; set; }

        public async Task InsertFlowAsync(FlowData flow)
        {
            if (DateTime.Now - _lastInsertTime >= TimeSpan.FromMilliseconds(Interval))
            {
                await FlowDataPersistenceTask.InsertFlowAsync(flow);
                _lastInsertTime = DateTime.Now;
            }
        }
    }

    class FlowDataPersistenceTask
    {
        private static readonly Channel<FlowData> _channel;
        private static readonly CancellationTokenSource _cancel;
        private static readonly List<FlowData> _flowBuffers;

        private static DateTime _lastUpdateTime = DateTime.Now;

        static FlowDataPersistenceTask()
        {
            _channel = Channel.CreateBounded<FlowData>(32);
            _flowBuffers = new List<FlowData>();
            _cancel = new CancellationTokenSource();

            StartInsertFlowTask();
        }

        public static bool IsRunning => !_cancel.IsCancellationRequested;

        public static async Task InsertFlowAsync(FlowData flow)
        {
            await _channel.Writer.WaitToWriteAsync();
            //LoggerHelper.WriteLog($"[QueueAdd]{flow.Address} {flow.CurrentFlow}");
            await _channel.Writer.WriteAsync(flow);
        }

        private static void StartInsertFlowTask()
        {
            Task.Factory.StartNew(async () =>
            {
                while (!_cancel.IsCancellationRequested &&
                    await _channel.Reader.WaitToReadAsync())
                {
                    var flow = await _channel.Reader.ReadAsync();
                    _flowBuffers.Add(flow);

                    //修改为500毫秒执行一次写入，使用事务提升写入速度
                    if (DateTime.Now - _lastUpdateTime > TimeSpan.FromMilliseconds(500) &&
                        _flowBuffers.Count > 0)
                    {
                        if (await SqliteHelper.InsertFlowsBatchAsync(_flowBuffers))
                        {
                            _flowBuffers.Clear();
                            _lastUpdateTime = DateTime.Now;
                        }
                    }
                }
            }, _cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }
}
