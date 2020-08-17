using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;
using MFCSoftware.Models;
using CommonLib.DbUtils;

namespace MFCSoftware.Common
{
    public static class DbStorage
    {
        private const string dbFile = "db.sqlite";
        private const string flowTableName = "tb_flow";
        private static readonly SqliteUtils utils;

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
                {"accu_flow", "float" },
                {"unit", "varchar(8)" }
            };
#if DEBUG
            //ClearDatabaseTable();
            //utils.DropTable(flowTableName);
#endif
            utils.CreateTableIfNotExist(flowTableName, flowTableTypes);
        }

        public static void InsertFlowData(int address, FlowData data)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"INSERT INTO {flowTableName}(address, collect_time, curr_flow, accu_flow, unit) VALUES (")
                .Append($"{address},")
                .Append("strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'),")
                .Append($"{data.CurrentFlow},")
                .Append($"{data.AccuFlow},")
                .Append($"'{data.Unit}');");
            string sql = sqlBuilder.ToString();
            utils.ExecuteNonQuery(sql);
        }

        public static List<FlowData> QueryLastest2HoursFlowData(int address)
        {
            DateTime now = DateTime.Now;
            string nowStr = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string twoHoursBeforeStr = now.AddHours(-2).ToString("yyyy-MM-dd HH:mm:ss.fff");

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT collect_time, curr_flow, accu_flow, unit FROM ")
                .Append(flowTableName)
                .Append($" WHERE address={address}")
                .Append($" AND collect_time BETWEEN '{twoHoursBeforeStr}' AND '{nowStr}'")
                .Append(" ORDER BY collect_time DESC;");
            string sql = sqlBuilder.ToString();

            List<FlowData> flows = new List<FlowData>();
            var reader = utils.ExecuteQuery(sql);
            while (reader.Read())
            {
                try
                {
                    var flow = new FlowData();
                    flow.CollectTime = reader.GetDateTime(0);
                    flow.CurrentFlow = reader.GetFloat(1);
                    flow.AccuFlow = reader.GetFloat(2);
                    flow.Unit = reader.GetString(3);
                    flows.Add(flow);
                }
                catch { continue; }
            }
            return flows;
        }

        private static void ClearDatabaseTable()
        {
            string sql = $"DELETE * FROM {flowTableName};";
            utils.ExecuteNonQuery(sql);
        }
    }
}
