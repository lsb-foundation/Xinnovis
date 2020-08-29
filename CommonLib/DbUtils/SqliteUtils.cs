using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace CommonLib.DbUtils
{
    public sealed class SqliteUtils : IDisposable
    {
        public SQLiteConnection Connection { get; }

        public SqliteUtils(string connectionString)
        {
            Connection = new SQLiteConnection(connectionString);
            Connection.Open();
        }

        private readonly object syncObject = new object();
        public int ExecuteNonQuery(string sql)
        {
            lock (syncObject)
            {
                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteQuery(string sql)
        {
            lock (syncObject)
            {
                using (var command = Connection.CreateCommand())
                {
                    command.CommandText = sql;
                    return command.ExecuteReader();
                }
            }
        }

        /// <summary>
        /// 创建数据表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fieldsWithType"></param>
        public void CreateTableIfNotExists(string tableName, Dictionary<string, string> fieldsWithType)
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

        /// <summary>
        /// 删除指定数据表
        /// </summary>
        /// <param name="tableName"></param>
        public void DropTableIfExists(string tableName)
        {
            string sql = $"DROP TABLE IF EXISTS {tableName};";
            ExecuteNonQuery(sql);
        }

        /// <summary>
        /// 删除指定数据表中所有数据
        /// </summary>
        /// <param name="tableName"></param>
        public void ClearTable(string tableName)
        {
            string sql = $"DELETE FROM {tableName};";
            ExecuteNonQuery(sql);
        }

        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }
    }
}
