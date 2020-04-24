using System;
using Judger.Managers;
using Judger.Models.Database;

namespace Judger.Core.Database.Internal.DbOperator
{
    public static class DbOperatorFactory
    {
        /// <summary>
        /// 根据数据库名创建主操作器
        /// 主操作器权限为root，负责新建库、创建新用户、分配权限等操作
        /// </summary>
        /// <param name="dbName">数据库名</param>
        public static BaseDbOperator CreateMainOperatorByName(string dbName)
        {
            if (dbName != DatabaseType.mysql.ToString())
                throw new NotImplementedException("Db operator not implemented: " + dbName);

            DbLangConfig config = DbManager.GetDbConfiguration(dbName);
            return Create(config);
        }

        /// <summary>
        /// 创建数据库操作器
        /// 注意：执行用户代码时使用的是由主操作器分配的无特权用户
        /// </summary>
        /// <param name="dbConfig">数据库配置</param>
        public static BaseDbOperator Create(DbLangConfig dbConfig)
        {
            string connString = dbConfig.ConnectionString;
            DbDriver driver = DbDriverLoader.Load(dbConfig.DriverPath);

            if (dbConfig.Name == DatabaseType.mysql.ToString())
                return new MySQL5xOperator(connString, driver);

            throw new NotImplementedException("Db operator not implemented: " + dbConfig.Name);
        }
    }
}