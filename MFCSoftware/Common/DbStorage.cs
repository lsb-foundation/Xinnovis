using Dapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MFCSoftware.Models;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;
using System.Data.SQLite;
using System.Data;

namespace MFCSoftware.Common
{
    public static class DbStorage
    {
        private const string dbFile = "db.sqlite";
        private static readonly string _connectionString;
        //private static readonly System.Timers.Timer timer;
        //private const int recordsMaxNumber = 100_0000;
        //private const int deleteCheckTime = 30;         //定时删除检查时间，单位：分钟

        static DbStorage()
        {
            var dbFilePath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _connectionString = string.Format("data source = {0}", dbFilePath);
            CreateTable();
            //timer = new System.Timers.Timer { AutoReset = true, Interval = deleteCheckTime * 60 * 1000 };
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();
        }

        //private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    //定时任务检查并删除数据库中超出的最早数据
        //    Task.Run(() =>
        //    {
        //        using (var transaction = utils.Connection.BeginTransaction())
        //        {
        //            string sql = $"SELECT COUNT(1) FROM {flowTableName};";
        //            var reader = utils.ExecuteQuery(sql);
        //            if (reader.Read())
        //            {
        //                int count = reader.GetInt32(0) - recordsMaxNumber;
        //                if (count > 0)
        //                {
        //                    sql = $"DELETE FROM {flowTableName} WHERE collect_time <= (SELECT MAX(collect_time) FROM " +
        //                          $"(SELECT collect_time FROM {flowTableName} ORDER BY collect_time LIMIT {count}));";
        //                    utils.ExecuteNonQuery(sql);
        //                    transaction.Commit();
        //                }
        //            }
        //        }
        //    });
        //}

        private static void CreateTable()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Execute("create table if not exists tb_flow(address int, collect_time datetime, curr_flow float, unit varchar(8), accu_flow float, accu_unit varchar(8));");
            }
        }

        public static async void InsertFlowData(int address, FlowData data)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.ExecuteAsync(
                    @"insert into tb_flow(address, collect_time, curr_flow, unit, accu_flow, accu_unit) 
                      values (@Address, @CollectTime, @CurrentFlow, @Unit, @AccuFlow, @AccuFlowUnit);",
                      new
                      {
                          Address = address,
                          CollectTime = DateTime.Now,
                          data.CurrentFlow,
                          Unit = data.Unit ?? string.Empty,
                          data.AccuFlow,
                          data.AccuFlowUnit
                      });
            }
        }

        public static async Task<List<FlowData>> QueryLastest2HoursFlowDatasAsync(int address)
        {
            DateTime now = DateTime.Now;
            DateTime twoHoursAgo = now.AddHours(-2);
            return await QueryFlowDatasByTimeAsync(twoHoursAgo, now, address);
        }

        public static async Task<List<FlowData>> QueryFlowDatasByTimeAsync(DateTime fromTime, DateTime toTime, int address)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                var flows = await connection.QueryAsync<FlowData>(
                    @"select collect_time as CollectTime, curr_flow as CurrentFlow, unit as Unit, accu_flow as AccuFlow, accu_unit as AccuFlowUnit
                      from tb_flow where address = @address and collect_time between @fromTime and @toTime order by collect_time;",
                      new
                      {
                          address,
                          fromTime,
                          toTime
                      });
                return flows.AsList();
            }
        }

        public static async Task<List<FlowData>> QueryAllFlowDatasAsync(int address)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                var flows = await connection.QueryAsync<FlowData>(
                    @"select collect_time as CollectTime, curr_flow as CurrentFlow, unit as Unit, accu_flow as AccuFlow, accu_unit as AccuFlowUnit
                      from tb_flow where address = @address",
                    new { address });
                return flows.AsList();
            }
        }
    }

    /// <summary>
    /// 按照一定间隔时间保存数据
    /// </summary>
    public class FlowDataSaver
    {
        private readonly int _address;
        private readonly System.Timers.Timer _timer;
        public FlowData Flow { get; set; }

        public FlowDataSaver(int address, int intervalMinutes)
        {
            _address = address;
            _timer = new System.Timers.Timer(TimeSpan.FromMinutes(intervalMinutes).TotalMilliseconds) { AutoReset = false };
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public void SetInterval(int minutes)
        {
            if (minutes > 0)
            {
                _timer.Interval = TimeSpan.FromMinutes((double)minutes).TotalMilliseconds;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Flow != null)
            {
                DbStorage.InsertFlowData(_address, Flow);
            }
            _timer.Start();
        }
    }

    /// <summary>
    /// 仅用于测试定时任务数据删除，不在正式运行的代码中起作用
    /// </summary>
    public static class DbStorageTest
    {
        private static readonly System.Timers.Timer timer = new System.Timers.Timer { AutoReset = false, Interval = 5 * 60 * 1000 };
        private static readonly CancellationTokenSource tokenSource;

        static DbStorageTest()
        {
            tokenSource = new CancellationTokenSource();
            timer.Elapsed += (s, e) => tokenSource.Cancel();
            timer.Start();
        }

        public static void Run()
        {
            Task.Run(() =>
            {
                Random random = new Random();
                float accuFlow = 0f;
                while (true)
                {
                    if (tokenSource.IsCancellationRequested) return;

                    float currFlow = (float)random.NextDouble();
                    accuFlow += currFlow;
                    FlowData flow = new FlowData
                    {
                        CurrentFlow = currFlow,
                        Unit = "SCCM",
                        AccuFlow = accuFlow,
                        AccuFlowUnit = "SCCM"
                    };
                    DbStorage.InsertFlowData(0, flow);
                    System.Threading.Thread.Sleep(1000*10);
                }
            }, tokenSource.Token);
        }
    }
}
