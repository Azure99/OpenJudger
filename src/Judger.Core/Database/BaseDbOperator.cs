using System;
using System.Data.Common;
using System.Text;

namespace Judger.Core.Database
{
    /// <summary>
    /// 数据库操作器基类
    /// </summary>
    public abstract class BaseDbOperator
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
        public abstract void CreateUser(string username);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="username">用户名</param>
        public abstract void DeleteUser(string username);


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
        public abstract void ExecuteNonQuery(string cmd);

        /// <summary>
        /// 执行命令并查询, 返回DataReader
        /// </summary>
        /// <param name="cmd">命令</param>
        /// <returns>读取查询结果的DataReader</returns>
        public abstract DbDataReader ExecuteReader(string cmd);
    }
}
