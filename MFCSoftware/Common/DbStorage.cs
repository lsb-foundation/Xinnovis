﻿using System;
using System.Collections.Generic;
using System.Text;
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
                {"unit", "varchar(8)" },
                {"accu_flow", "float" },
                {"accu_unit", "varchar(8)" }
            };

            //utils.DropTableIfExists(flowTableName);
            utils.CreateTableIfNotExists(flowTableName, flowTableTypes);
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

        public static List<FlowData> QueryLastest2HoursFlowDatas(int address)
        {
            DateTime now = DateTime.Now;
            DateTime twoHoursAgo = now.AddHours(-2);
            return QueryFlowDatas(twoHoursAgo, now, address);
        }

        public static List<FlowData> QueryFlowDatas(DateTime fromTime, DateTime toTime, int address)
        {
            string fromTimeStr = fromTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string toTimeStr = toTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"SELECT collect_time, curr_flow, unit, accu_flow, accu_unit FROM {flowTableName}")
                .Append($" WHERE address={address}")
                .Append($" AND collect_time BETWEEN '{toTimeStr}' AND '{fromTimeStr}'")
                .Append(" ORDER BY collect_time;");

            return QueryFlowDatasBySql(sqlBuilder.ToString());
        }

        public static List<FlowData> QueryAllFlowDatas(int address)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"SELECT collect_time, curr_flow, unit, accu_flow, accu_unit FROM {flowTableName}")
                .Append($" WHERE address={address}")
                .Append(" ORDER BY collect_time;");

            return QueryFlowDatasBySql(sqlBuilder.ToString());
        }

        private static List<FlowData> QueryFlowDatasBySql(string sql)
        {
            List<FlowData> flows = new List<FlowData>();
            var reader = utils.ExecuteQuery(sql);
            while (reader.Read())
            {
                try
                {
                    var flow = new FlowData();
                    flow.CollectTime = reader.GetDateTime(0);
                    flow.CurrentFlow = reader.GetFloat(1);
                    flow.Unit = reader.GetString(2);
                    flow.AccuFlow = reader.GetFloat(3);
                    flow.AccuFlowUnit = reader.GetString(4);
                    flows.Add(flow);
                }
                catch { continue; }
            }
            return flows;
        }
    }
}
