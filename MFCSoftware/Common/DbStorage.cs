using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;
using MFCSoftware.Models;

namespace MFCSoftware.Common
{
    public static class DbStorage
    {
        private static readonly string dbFile = "db.sqlite";
        private static readonly string flowTableName = "tb_flow";
        private static string connectionString;
        private static SQLiteConnection connection;

        static DbStorage()
        {
            var dbFilePath = Path.Combine(Environment.CurrentDirectory, dbFile);
            connectionString = string.Format("data source = {0}", dbFilePath);
            connection = new SQLiteConnection(connectionString);

            var flowTableTypes = new Dictionary<string, string>()
            {
                {"address", "int" },
                {"collect_time", "datetime" },
                {"curr_flow", "float" },
                {"accu_flow", "float" },
                {"unit", "varchar(8)" }
            };
#if DEBUG
            //DropTable(flowTableName);
#endif
            CreateTableIfNotExist(flowTableName, flowTableTypes);
        }

        public static void InsertFlowData(int address, FlowData data)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO tb_flow(address, collect_time, curr_flow, accu_flow, unit) VALUES (")
                .Append($"{address},")
                .Append("strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'),")
                .Append($"{data.CurrentFlow},")
                .Append($"{data.AccuFlow},")
                .Append($"'{data.Unit}');");
            string sql = sqlBuilder.ToString();
            ExecuteNonQuery(sql);
        }

        public static List<FlowData> QueryLastest2HoursFlowData(int address)
        {
            DateTime now = DateTime.Now;
            string nowStr = now.ToString("yyyy-MM-dd hh:mm:ss.fff");
            string twoHoursBeforeStr = now.AddHours(-2).ToString("yyyy-MM-dd hh:mm:ss.fff");

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT collect_time, curr_flow, accu_flow, unit FROM ")
                .Append(flowTableName)
                .Append($" WHERE address={address}")
                .Append($" AND collect_time BETWEEN '{twoHoursBeforeStr}' AND '{nowStr}'")
                .Append(" ORDER BY collect_time DESC;");
            string sql = sqlBuilder.ToString();

            List<FlowData> flows = new List<FlowData>();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                var reader = command.ExecuteReader();
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
            }
            connection.Close();
            return flows;
        }

        private static object lockObj = new object();
        private static int ExecuteNonQuery(string sql)
        {
            lock (lockObj)
            {
                connection.Open();
                int ret = 0;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    ret = command.ExecuteNonQuery();
                }
                connection.Close();
                return ret;
            }
        }

        private static void CreateTableIfNotExist(string tableName, Dictionary<string, string> fieldsWithType)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("CREATE TABLE IF NOT EXISTS ").Append(tableName).Append("(");
            foreach (var kv in fieldsWithType)
            {
                string field = kv.Key;
                string type = kv.Value;
                if (string.IsNullOrWhiteSpace(field)) continue;
                if (string.IsNullOrWhiteSpace(type)) continue;
                sqlBuilder.Append($"{field} {type},");
            }
            int lastIndex = sqlBuilder.Length - 1;
            if (sqlBuilder[lastIndex] == ',')
                sqlBuilder.Remove(lastIndex, 1);
            sqlBuilder.Append(");");
            string sql = sqlBuilder.ToString();
            ExecuteNonQuery(sql);
        }

        private static void DropTable(string tableName)
        {
            string sql = $"DROP TABLE IF EXISTS {tableName};";
            ExecuteNonQuery(sql);
        }
    }
}
