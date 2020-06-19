using System;
using System.Collections.Generic;
using System.Data.Common;
using Judger.Core.Database.Internal.Entity;

namespace Judger.Core.Database.Internal.DbOperator
{
    /// <summary>
    /// 数据库操作器基类
    /// </summary>
    /// 创建时提供指定数据库的连接字符串和数据库驱动
    /// 实现类应当封装对应数据库的基本操作
    public abstract class BaseDbOperator : IDisposable
    {
        protected BaseDbOperator(string connectionString, DbDriver driver)
        {
            ConnectionString = connectionString;
            DbDriver = driver;
        }

        protected string ConnectionString { get; }

        protected DbDriver DbDriver { get; }

        public abstract void Dispose();

        /// <summary>
        /// 创建数据库
        /// </summary>
        public abstract void CreateDatabase(string database);

        /// <summary>
        /// 删除数据库
        /// </summary>
        public abstract void DropDatabase(string database);

        /// <summary>
        /// 创建用户
        /// </summary>
        public abstract void CreateUser(string username, string password);

        /// <summary>
        /// 删除用户
        /// </summary>
        public abstract void DropUser(string username);

        /// <summary>
        /// 向用户授权数据库
        /// </summary>
        public abstract void GrantPrivileges(string database, string username);

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="cmd">命令</param>
        public abstract void InitDatabase(string cmd);

        /// <summary>
        /// 执行命令不查询
        /// </summary>
        /// <returns>操作影响的行数</returns>
        public abstract int ExecuteNonQuery(string cmd, int timeout = 0);

        /// <summary>
        /// 执行命令并查询, 返回DataReader
        /// </summary>
        public abstract DbDataReader ExecuteQuery(string cmd, int timeout = 0);

        /// <summary>
        /// 读取库中所有表的所有数据
        /// </summary>
        public abstract DbData ReadDbData();

        /// <summary>
        /// 从Reader中读取此次查询的所有数据
        /// </summary>
        public static DbQueryData ReadQueryData(DbDataReader reader, string name = "")
        {
            int fieldCount = reader.FieldCount;
            string[] fieldNames = new string[fieldCount];
            for (int i = 0; i < fieldCount; i++)
                fieldNames[i] = reader.GetName(i);

            List<string[]> records = new List<string[]>();
            while (reader.Read())
            {
                string[] record = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    if (reader.IsDBNull(i))
                        record[i] = null;
                    else if (reader.GetFieldType(i) == typeof(DateTime))
                        record[i] = null;
                    else
                        record[i] = reader.GetString(i);
                }

                records.Add(record);
            }

            return new DbQueryData
            {
                Name = name,
                FieldNames = fieldNames,
                Records = records
            };
        }
    }
}