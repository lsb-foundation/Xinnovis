using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MFCSoftware.Models;
using CommonLib.DbUtils;
using System.Timers;
using System.Threading.Tasks;
using System.Threading;

namespace MFCSoftware.Common
{
    public static class DbStorage
    {
        private const string dbFile = "db.sqlite";
        private const string flowTableName = "tb_flow";
        private static readonly SqliteUtils utils;
        private static readonly System.Timers.Timer timer;
        private const int recordsMaxNumber = 100_0000;
        private const int deleteCheckTime = 30;         //定时删除检查时间，单位：分钟

        static DbStorage()
        {
            var dbFilePath = Path.Combine(Environment.CurrentDirectory, dbFile);
            var connectionString = string.Format("data source = {0}", dbFilePath);
            utils = new SqliteUtils(connectionString);

            var flowTableTypes = new Dictionary<string, string>()
            {
                {"address", "int" },
                {"collect_time", "datetime" },
                {"curr_flow", "float" },
                {"unit", "varchar(8)" },
                {"accu_flow", "float" },
                {"accu_unit", "varchar(8)" }
            };

            utils.CreateTableIfNotExists(flowTableName, flowTableTypes);

            timer = new System.Timers.Timer { AutoReset = true, Interval = deleteCheckTime * 60 * 1000 };
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //定时任务检查并删除数据库中超出的最早数据
            Task.Run(() =>
            {
                using (var transaction = utils.Connection.BeginTransaction())
                {
                    string sql = $"SELECT COUNT(1) FROM {flowTableName};";
                    var reader = utils.ExecuteQuery(sql);
                    if (reader.Read())
                    {
                        int count = reader.GetInt32(0) - recordsMaxNumber;
                        if (count > 0)
                        {
                            sql = $"DELETE FROM {flowTableName} WHERE collect_time <= (SELECT MAX(collect_time) FROM " +
                                  $"(SELECT collect_time FROM {flowTableName} ORDER BY collect_time LIMIT {count}));";
                            utils.ExecuteNonQuery(sql);
                            transaction.Commit();
                        }
                    }
                }
            });
        }

        public static void InsertFlowDatasByTransaction(int address, List<FlowData> flows)
        {
            using(var transaction = utils.Connection.BeginTransaction())
            {
                foreach (var data in flows)
                    InsertFlowData(address, data);
                transaction.Commit();
            }
        }

        public static void InsertFlowData(int address, FlowData data)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"INSERT INTO {flowTableName}(address, collect_time, curr_flow, unit, accu_flow, accu_unit) VALUES (")
                .Append($"{address},")
                .Append("strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'),")
                .Append($"{data.CurrentFlow},")
                .Append($"'{data.Unit}',")
                .Append($"{data.AccuFlow},")
                .Append($"'{data.AccuFlowUnit}');");
            string sql = sqlBuilder.ToString();
            utils.ExecuteNonQuery(sql);
        }

        public static async Task<List<FlowData>> QueryLastest2HoursFlowDatasAsync(int address)
        {
            DateTime now = DateTime.Now;
            DateTime twoHoursAgo = now.AddHours(-2);
            return await QueryFlowDatasByTimeAsync(twoHoursAgo, now, address);
        }

        public static async Task<List<FlowData>> QueryFlowDatasByTimeAsync(DateTime fromTime, DateTime toTime, int address)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"SELECT collect_time, curr_flow, unit, accu_flow, accu_unit FROM {flowTableName}")
                .Append($" WHERE address={address}")
                .Append($" AND collect_time BETWEEN '{fromTime:yyyy-MM-dd HH:mm:ss.fff}' AND '{toTime:yyyy-MM-dd HH:mm:ss.fff}'")
                .Append(" ORDER BY collect_time;");

            return await QueryFlowDatasBySqlAsync(sqlBuilder.ToString());
        }

        public static async Task<List<FlowData>> QueryAllFlowDatasAsync(int address)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"SELECT collect_time, curr_flow, unit, accu_flow, accu_unit FROM {flowTableName}")
                .Append($" WHERE address={address}")
                .Append(" ORDER BY collect_time;");

            return await QueryFlowDatasBySqlAsync(sqlBuilder.ToString());
        }

        private static async Task<List<FlowData>> QueryFlowDatasBySqlAsync(string sql)
        {
            var task = new Task<List<FlowData>>(() =>
            {
                List<FlowData> flows = new List<FlowData>();
                var reader = utils.ExecuteQuery(sql);
                while (reader.Read())
                {
                    try
                    {
                        var flow = new FlowData
                        {
                            CollectTime = reader.GetDateTime(0),
                            CurrentFlow = reader.GetFloat(1),
                            Unit = reader.GetString(2),
                            AccuFlow = reader.GetFloat(3),
                            AccuFlowUnit = reader.GetString(4)
                        };
                        flows.Add(flow);
                    }
                    catch { }
                }
                return flows;
            });
            task.Start();
            return await task;
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
