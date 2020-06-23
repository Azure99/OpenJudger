using System;
using System.Collections.Generic;
using System.Data.Common;
using Judger.Core.Database.Internal.Entity;
using Judger.Utils;

namespace Judger.Core.Database.Internal.DbOperator
{
    /// <summary>
    /// MySQL操作器
    /// </summary>
    public class MySqlOperator : BaseDbOperator
    {
        private readonly DbConnection _connection;

        public MySqlOperator(string connectionString, DbDriver driver) : base(connectionString, driver)
        {
            _connection = DbDriver.CreateConnection(ConnectionString);
        }

        public override void CreateDatabase(string database)
        {
            string cmd = $"CREATE DATABASE {database}";
            ExecuteNonQuery(cmd);
        }

        public override void DropDatabase(string database)
        {
            string cmd = $"DROP DATABASE {database}";
            ExecuteNonQuery(cmd);
        }

        public override void CreateUser(string username, string password)
        {
            string cmd = $"CREATE USER '{username}'@'localhost' IDENTIFIED BY '{password}'";
            ExecuteNonQuery(cmd);
        }

        public override void DropUser(string username)
        {
            string cmd = $"DROP USER '{username}'@'localhost'";
            ExecuteNonQuery(cmd);
        }

        public override void GrantPrivileges(string database, string username)
        {
            string cmd = $"GRANT ALL ON {database}.* to '{username}'@'localhost'";
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
                command.CommandTimeout = timeout;

            return command.ExecuteNonQuery();
        }

        public override DbDataReader ExecuteQuery(string cmd, int timeout = 0)
        {
            DbCommand command = DbDriver.CreateCommand(cmd, _connection);
            if (timeout > 0)
                command.CommandTimeout = timeout;

            return command.ExecuteReader();
        }

        public override DbData ReadDbData()
        {
            string[] tablesName = GetAllTablesName();
            DbQueryData[] data = new DbQueryData[tablesName.Length];
            for (int i = 0; i < tablesName.Length; i++)
                data[i] = GetTableData(tablesName[i]);

            Array.Sort(data, (a, b) => a.Name.CompareToOrdinal(b.Name));

            return new DbData
            {
                TablesData = data
            };
        }

        private string[] GetAllTablesName()
        {
            DbDataReader reader = ExecuteQuery("SHOW TABLES");

            List<string> tables = new List<string>();
            while (reader.Read())
                tables.Add(reader.GetString(0));

            return tables.ToArray();
        }

        private DbQueryData GetTableData(string tableName)
        {
            string cmd = $"SELECT * FROM {tableName}";

            DbDataReader reader = ExecuteQuery(cmd);
            DbQueryData queryData = ReadQueryData(reader, tableName);

            return queryData;
        }

        public override void Dispose()
        {
            _connection.Close();
        }
    }
}