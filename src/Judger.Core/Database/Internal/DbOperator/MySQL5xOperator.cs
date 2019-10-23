﻿using System;
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
        private readonly DbConnection _connection;

        public MySQL5xOperator(string connectionString, DbDriver driver) : base(connectionString, driver)
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

        public override void GeneratePrivileges(string database, string username)
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

        public override DbDataReader ExecuteReader(string cmd, int timeout = 0)
        {
            DbCommand command = DbDriver.CreateCommand(cmd, _connection);
            if (timeout > 0)
                command.CommandTimeout = timeout;

            return command.ExecuteReader();
        }

        public override DbData ReadDbData()
        {
            string[] tablesName = GetAllTablesName();
            var data = new DbQueryData[tablesName.Length];
            for (var i = 0; i < tablesName.Length; i++)
                data[i] = GetTableData(tablesName[i]);

            Array.Sort(data, (a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

            return new DbData
            {
                TablesData = data
            };
        }

        private string[] GetAllTablesName()
        {
            DbDataReader reader = ExecuteReader("SHOW TABLES");

            var tables = new List<string>();
            while (reader.Read())
                tables.Add(reader.GetString(0));

            return tables.ToArray();
        }

        /// <summary>
        /// 查询表的所有记录
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>表数据</returns>
        private DbQueryData GetTableData(string tableName)
        {
            string cmd = $"SELECT * FROM {tableName}";
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