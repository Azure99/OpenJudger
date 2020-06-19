using System;
using System.Data.Common;
using System.Reflection;

namespace Judger.Core.Database.Internal
{
    /// <summary>
    /// 数据库驱动
    /// </summary>
    public class DbDriver
    {
        /// <summary>
        /// 数据库驱动
        /// </summary>
        /// <param name="dbConnection">DbConnection类</param>
        /// <param name="dbCommand">DbCommand类</param>
        public DbDriver(Type dbConnection, Type dbCommand)
        {
            DbConnectionType = dbConnection;
            DbCommandType = dbCommand;
        }

        private Type DbConnectionType { get; }

        private Type DbCommandType { get; }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="openConnection">创建后是否立即打开连接</param>
        public DbConnection CreateConnection(string connectionString, bool openConnection = true)
        {
            object[] args = {connectionString};
            DbConnection conn = CreateInstance(DbConnectionType.FullName, args) as DbConnection;
            if (conn != null && openConnection)
                conn.Open();

            return conn;
        }

        /// <summary>
        /// 创建数据库命令
        /// </summary>
        /// <param name="cmdText">命令(文本)</param>
        /// <param name="connection">数据库连接</param>
        public DbCommand CreateCommand(string cmdText, DbConnection connection)
        {
            object[] args = {cmdText, connection};
            return CreateInstance(DbCommandType.FullName, args) as DbCommand;
        }

        private object CreateInstance(string fullName, object[] args)
        {
            return DbCommandType.Assembly.CreateInstance(
                fullName, true, BindingFlags.Default, null, args, null, null);
        }
    }
}