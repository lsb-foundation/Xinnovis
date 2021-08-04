using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Collections.Concurrent;
using System.Threading;
using CommonLib.Extensions;

namespace MFCSoftware.Utils
{
    public static class DbStorage
    {
        private const string dbFile = "db.sqlite";
        private static readonly string _connectionString;
        private static readonly ConcurrentQueue<FlowData> _queue;

        static DbStorage()
        {
            string dbFilePath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _connectionString = string.Format("data source = {0}", dbFilePath);
            _queue = new ConcurrentQueue<FlowData>();
            CreateTable();
            _ = Task.Run(() => InsertFlowFromQueue());
        }

        private async static void CreateTable()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                _ = connection.Execute("create table if not exists tb_flow(address int, collect_time datetime, curr_flow float, unit varchar(8), accu_flow float, accu_unit varchar(8));");
                _ = connection.Execute("create index if not exists idx_tb_flow_addr on tb_flow(address);");
                _ = connection.Execute("create index if not exists idx_tb_flow_time on tb_flow(collect_time);");
                _ = connection.Execute("create table if not exists tb_password(password varchar(64));");
                if ((await connection.QueryFirstAsync<int>("select count(1) from tb_password")) == 0)
                {
                    string defaultPassword = "123456".MD5HashString();
                    _ = connection.Execute("insert into tb_password(password) values(@password);", new { password = defaultPassword});
                }
            }
        }

        private static async void InsertFlowFromQueue()
        {
            while (true)
            {
                if (_queue.TryDequeue(out FlowData flow))
                {
                    using (var connection = new SQLiteConnection(_connectionString))
                    {
                        _ = await connection.ExecuteAsync(
                            @"insert into tb_flow(address, collect_time, curr_flow, unit, accu_flow, accu_unit) 
                              values (@Address, @CollectTime, @CurrentFlow, @Unit, @AccuFlow, @AccuFlowUnit);",
                              new
                              {
                                  flow.Address,
                                  CollectTime = DateTime.Now,
                                  flow.CurrentFlow,
                                  Unit = flow.Unit ?? string.Empty,
                                  flow.AccuFlow,
                                  flow.AccuFlowUnit
                              });
                    }
                }
                Thread.Sleep(5);
            }
        }

        public static void InsertFlowData(FlowData data) => _queue.Enqueue(data);

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

        public static async Task<List<FlowData>> QueryLatestAccumulateFlowDatasAsync()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                var flows = await connection.QueryAsync<FlowData>(
                    @"select address as Address, collect_time as CollectTime, accu_flow as AccuFlow, accu_unit as AccuFlowUnit from (
                     	select *, rank() over(partition by address order by collect_time desc) rk from tb_flow
                     ) where rk = 1");
                return flows.AsList();
            }
        }

        public static async Task<bool> CheckPasswordAsync(string password)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                string md5Password = await connection.QueryFirstAsync<string>("select password from tb_password");
                string hashedPassword = password.MD5HashString();
                return md5Password == hashedPassword;
            }
        }

        public static async Task UpdatePasswordAsync(string password)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                string md5Password = password.MD5HashString();
                _ = await connection.ExecuteAsync("update tb_password set password = @Password;",
                    new { Password = md5Password });
            }
        }
    }
}