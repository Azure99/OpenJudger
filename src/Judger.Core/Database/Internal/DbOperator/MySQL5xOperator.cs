using System;
using System.Collections.Generic;
using System.Data.Common;
using Judger.Core.Database.Internal.Entity;

namespace Judger.Core.Database.Internal.DbOperator
{
    /// <summary>
    /// MySQL5.x操作器
    /// </summary>
    public class MySQL5xOperator : BaseDbOperator
    {
        private DbConnection _connection;
        public MySQL5xOperator(string connectionString, DbDriver driver) : base(connectionString, driver)
        {
            _connection = DbDriver.CreateConnection(ConnectionString);
        }

        public override void CreateDatabase(string database)
        {
            string cmd = string.Format("CREATE DATABASE {0}", database);
            ExecuteNonQuery(cmd);
        }

        public override void DropDatabase(string database)
        {
            string cmd = string.Format("DROP DATABASE {0}", database);
            ExecuteNonQuery(cmd);
        }

        public override void CreateUser(string username, string password)
        {
            string cmd = string.Format("CREATE USER '{0}'@'localhost' IDENTIFIED BY '{1}'", username, password);
            ExecuteNonQuery(cmd);
        }

        public override void DropUser(string username)
        {
            string cmd = string.Format("DROP USER '{0}'@'localhost'", username);
            ExecuteNonQuery(cmd);
        }

        public override void GeneratePrivileges(string database, string username)
        {
            string cmd = string.Format("GRANT ALL ON {0}.* to '{1}'@'localhost'", database, username);
            ExecuteNonQuery(cmd);
        }

        public override void InitDatabase(string cmd)
        {
            ExecuteNonQuery(cmd);
        }

        public override int ExecuteNonQuery(string cmd, int timeout = 0)
        {
            DbCommand command = DbDriver.CreateCommand(cmd, _connection);
            if (timeout > 0)
            {
                command.CommandTimeout = timeout;
            }

            return command.ExecuteNonQuery();
        }

        public override DbDataReader ExecuteReader(string cmd, int timeout = 0)
        {
            DbCommand command = DbDriver.CreateCommand(cmd, _connection);
            if (timeout > 0)
            {
                command.CommandTimeout = timeout;
            }

            return command.ExecuteReader();
        }

        public override DbData ReadDbData()
        {
            string[] tablesName = GetAllTablesName();
            DbQueryData[] datas = new DbQueryData[tablesName.Length];
            for (int i = 0; i < tablesName.Length; i++)
            {
                datas[i] = GetTableData(tablesName[i]);
            }

            Array.Sort(datas, (a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

            return new DbData
            {
                TablesData = datas
            };
        }

        /// <summary>
        /// 获取所有表的名称
        /// </summary>
        /// <returns>表名</returns>
        private string[] GetAllTablesName()
        {
            DbDataReader reader = ExecuteReader("SHOW TABLES");

            List<string> tables = new List<string>();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            return tables.ToArray();
        }

        /// <summary>
        /// 查询表的所有记录
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>表数据</returns>
        private DbQueryData GetTableData(string tableName)
        {
            string cmd = string.Format("SELECT * FROM {0}", tableName);
            DbDataReader reader = ExecuteReader(cmd);

            DbQueryData queryData = ReadQueryData(reader, tableName);

            return queryData;
        }

        public override void Dispose()
        {
            _connection.Close();
        }
    }
}
