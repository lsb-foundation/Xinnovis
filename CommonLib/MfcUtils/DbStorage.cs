using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.Concurrent;
using System.Threading;

namespace CommonLib.MfcUtils
{
    public static class DbStorage
    {
        private const string dbFile = "db.sqlite";
        private static readonly string _connectionString;
        private static readonly ConcurrentQueue<QueueModel> _queue;

        static DbStorage()
        {
            string dbFilePath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _connectionString = string.Format("data source = {0}", dbFilePath);
            _queue = new ConcurrentQueue<QueueModel>();
            CreateTable();
            _ = Task.Run(() => InsertFlowFromQueue());
        }

        private static void CreateTable()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Execute("create table if not exists tb_flow(address int, collect_time datetime, curr_flow float, unit varchar(8), accu_flow float, accu_unit varchar(8));");
                connection.Execute("create index if not exists idx_tb_flow_addr on tb_flow(address);");
                connection.Execute("create index if not exists idx_tb_flow_time on tb_flow(collect_time);");
            }
        }

        private static async void InsertFlowFromQueue()
        {
            while (true)
            {
                if (_queue.TryDequeue(out QueueModel model))
                {
                    using (var connection = new SQLiteConnection(_connectionString))
                    {
                        _ = await connection.ExecuteAsync(
                            @"insert into tb_flow(address, collect_time, curr_flow, unit, accu_flow, accu_unit) 
                              values (@Address, @CollectTime, @CurrentFlow, @Unit, @AccuFlow, @AccuFlowUnit);",
                              new
                              {
                                  model.Address,
                                  CollectTime = DateTime.Now,
                                  model.Flow.CurrentFlow,
                                  Unit = model.Flow.Unit ?? string.Empty,
                                  model.Flow.AccuFlow,
                                  model.Flow.AccuFlowUnit
                              });
                    }
                }
                Thread.Sleep(5);
            }
        }

        public static void InsertFlowData(int address, FlowData data)
        {
            _queue.Enqueue(new QueueModel
            {
                Address = address,
                Flow = data
            });
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
    
    class QueueModel
    {
        public int Address;
        public FlowData Flow;
    }
}