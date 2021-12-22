using Dapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using CommonLib.Extensions;
using CommonLib.Utils;

namespace MFCSoftware.Utils
{
    public static class SqliteHelper
    {
        private const string dbFile = "db.sqlite";
        private static readonly string _connectionString;

        static SqliteHelper()
        {
            string dbFilePath = Path.Combine(Environment.CurrentDirectory, dbFile);
            _connectionString = string.Format("data source = {0}", dbFilePath);
            CreateTables();
        }

        private static void CreateTables()
        {
            CreateFlowTable();
            CreatePasswordTable();
            CreateSettingsTable();
        }

        private async static void CreateFlowTable()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.ExecuteAsync(
                "create table if not exists tb_flow(address int, collect_time datetime, curr_flow float, unit varchar(8), accu_flow float, accu_unit varchar(8), temperature float);");
            await connection.ExecuteAsync("create index if not exists idx_tb_flow_addr on tb_flow(address);");
            await connection.ExecuteAsync("create index if not exists idx_tb_flow_time on tb_flow(collect_time);");
        }

        private async static void CreatePasswordTable()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.ExecuteAsync("create table if not exists tb_password(password varchar(64));");
            if ((await connection.QuerySingleAsync<int>("select count(1) from tb_password")) == 0)
            {
                string defaultPassword = "123456".MD5HashString();
                _ = await connection.ExecuteAsync("insert into tb_password(password) values(@password);", new
                {
                    password = defaultPassword
                });
            }
        }

        private async static void CreateSettingsTable()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.ExecuteAsync("create table if not exists tb_settings(s_key varchar(32), s_value varchar(32));");
        }

        public static async Task<bool> InsertFlowsBatchAsync(List<FlowData> flows)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var trans = connection.BeginTransaction();
            try
            {
                foreach (var flow in flows)
                {
                    await connection.ExecuteAsync(
                        @"insert into tb_flow(address, collect_time, curr_flow, unit, accu_flow, accu_unit, temperature) 
                              values (@Address, @CollectTime, @CurrentFlow, @Unit, @AccuFlow, @AccuFlowUnit, @Temperature);",
                          new
                          {
                              flow.Address,
                              flow.CollectTime,
                              flow.CurrentFlow,
                              Unit = flow.Unit ?? string.Empty,
                              flow.AccuFlow,
                              flow.AccuFlowUnit,
                              flow.Temperature
                          });
                    //LoggerHelper.WriteLog($"[DatabaseInsert]{flow.Address} {flow.CurrentFlow}");
                }
                trans.Commit();
                return true;
            }
            catch (SQLiteException e) //客户反馈此处发生SqliteException异常(经核实，偶尔会出现attempt to write a readonly database异常，频率较低)
            {
                trans.Rollback();
                LoggerHelper.WriteLog(e.Message, e);
                return false;
            }
        }

        public static async Task<List<FlowData>> QueryFlowDatasByTimeAsync(DateTime fromTime, DateTime toTime, int address)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                var flows = await connection.QueryAsync<FlowData>(
                    @"select collect_time as CollectTime, curr_flow as CurrentFlow, unit as Unit, accu_flow as AccuFlow, accu_unit as AccuFlowUnit, temperature as Temperature
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
                    @"select collect_time as CollectTime, curr_flow as CurrentFlow, unit as Unit, accu_flow as AccuFlow, accu_unit as AccuFlowUnit, temperature as Temperature
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

        public static async Task<string> GetSettings(string key)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<string>(
                    "select s_value from tb_settings where s_key = @key", new { key });
            }
        }

        public static async Task UpdateSettings(string key, string value)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                if (string.IsNullOrEmpty(await GetSettings(key)))
                {
                    _ = await connection.ExecuteAsync(
                        "insert into tb_settings(s_key, s_value) values(@key, @value);",
                        new { key, value });
                }
                else
                {
                    _ = await connection.ExecuteAsync(
                        "update tb_settings set s_value = @value where s_key = @key;",
                        new { value, key });
                }
            }
        }
    }
}