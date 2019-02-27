using System;
using System.Data.Common;
using System.Reflection;

namespace Judger.Core.Database
{
    public class DatabaseDriverLoader
    {
        private string _path;
        private Assembly _assembly;

        /// <summary>
        /// DbConnection
        /// </summary>
        public Type ConnectionType { get; private set; }

        /// <summary>
        /// DbCommand
        /// </summary>
        public Type CommandType { get; private set; }

        /// <summary>
        /// 数据库驱动加载器
        /// </summary>
        /// <param name="path">数据库驱动路径</param>
        public DatabaseDriverLoader(string path)
        {
            _path = path;
            LoadDriver();
        }

        private void LoadDriver()
        {
            _assembly = Assembly.LoadFile(_path);
            var types = _assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsClass)
                {
                    continue;
                }

                if (type.BaseType == typeof(DbConnection))
                {
                    ConnectionType = type;
                }
                else if (type.BaseType == typeof(DbCommand))
                {
                    CommandType = type;
                }
            }

            if (ConnectionType == null || CommandType == null)
            {
                throw new Exception("Driver unavailable!");
            }
        }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="openConnection">创建后是否打开连接</param>
        /// <returns></returns>
        public DbConnection CreateConnection(string connectionString, bool openConnection = true)
        {
            object[] args = new object[1] { connectionString };
            DbConnection conn = CreateInstance(ConnectionType.FullName, args) as DbConnection;
            if (conn != null && openConnection)
            {
                conn.Open();
            }

            return conn;
        }

        /// <summary>
        /// 创建数据库命令
        /// </summary>
        /// <param name="cmdText">命令</param>
        /// <param name="connection">数据库连接</param>
        /// <returns></returns>
        public DbCommand CreateCommand(string cmdText, DbConnection connection)
        {
            object[] args = new object[2] { cmdText, connection };
            DbCommand command = CreateInstance(CommandType.FullName, args) as DbCommand;

            return command;
        }

        private object CreateInstance(string fullName, object[] args)
        {
            return _assembly.CreateInstance(fullName, true, BindingFlags.Default, null, args, null, null);
        }
    }
}
