using System;
using System.Collections.Generic;
using System.Data.Common;
using Judger.Core.Database.Internal.Entity;

namespace Judger.Core.Database.Internal
{
    /// <summary>
    /// 数据库操作器基类
    /// </summary>
    public abstract class BaseDbOperator : IDisposable
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// 数据库驱动
        /// </summary>
        public DbDriver DbDriver { get; private set; }

        /// <summary>
        /// 数据库操作器基类
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="driver">数据库驱动</param>
        public BaseDbOperator(string connectionString, DbDriver driver)
        {
            ConnectionString = connectionString;
            DbDriver = driver;
        }

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="database">数据库名</param>
        public abstract void CreateDatabase(string database);

        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="database">数据库名</param>
        public abstract void DropDatabase(string database);

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public abstract void CreateUser(string username, string password);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="username">用户名</param>
        public abstract void DropUser(string username);

        /// <summary>
        /// 向用户授权数据库
        /// </summary>
        /// <param name="database">数据库名</param>
        /// <param name="username">用户名</param>
        public abstract void GeneratePrivileges(string database, string username);

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="cmd">命令</param>
        public abstract void InitDatabase(string cmd);

        /// <summary>
        /// 执行命令不查询
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>操作影响的行数</returns>
        public abstract int ExecuteNonQuery(string cmd, int timeout = 0);

        /// <summary>
        /// 执行命令并查询, 返回DataReader
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>读取查询结果的DataReader</returns>
        public abstract DbDataReader ExecuteReader(string cmd, int timeout = 0);

        /// <summary>
        /// 读取库中所有表的所有数据
        /// </summary>
        /// <returns>全部数据</returns>
        public abstract DbData ReadDbData();

        /// <summary>
        /// 从Reader中读取此次查询的所有数据
        /// </summary>
        /// <returns>查询数据</returns>
        public static DbQueryData ReadQueryData(DbDataReader reader, string name = "")
        {
            int fieldCount = reader.FieldCount;
            string[] fieldNames = new string[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                fieldNames[i] = reader.GetName(i);
            }

            List<string[]> records = new List<string[]>();
            while (reader.Read())
            {
                string[] record = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    if (reader.IsDBNull(i))
                    {
                        record[i] = null;
                    }
                    else if (reader.GetFieldType(i) == typeof(DateTime))
                    {
                        record[i] = null;
                    }
                    else
                    {
                        record[i] = reader.GetString(i);
                    }
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

        public abstract void Dispose();
    }
}