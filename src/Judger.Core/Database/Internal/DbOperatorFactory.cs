using System;
using Judger.Core.Database.Internal.DbOperator;
using Judger.Entity;
using Judger.Managers;

namespace Judger.Core.Database.Internal
{
    public static class DbOperatorFactory
    {
        /// <summary>
        /// 根据数据库名创建主操作器
        /// </summary>
        /// <param name="dbName">数据库名</param>
        /// <returns>数据库操作器</returns>
        public static BaseDbOperator CreateMainOperatorByName(string dbName)
        {
            if (dbName == DatabaseType.Mysql.ToString())
            {
                DbConfiguration config = DbManager.GetDbConfiguration(dbName);
                return Create(config);
            }
            else
            {
                throw new NotImplementedException("Db operator not implemented: " + dbName);
            }
        }

        /// <summary>
        /// 创建数据库操作器
        /// </summary>
        /// <param name="dbConfig">数据库配置</param>
        /// <returns>数据库操作器</returns>
        public static BaseDbOperator Create(DbConfiguration dbConfig)
        {
            string connString = dbConfig.ConnectionString;
            DbDriver driver = DbDriverLoader.Load(dbConfig.DriverPath);

            if (dbConfig.DbName == DatabaseType.Mysql.ToString()) 
            {
                return new MySQL5xOperator(connString, driver);
            }
            else
            {
                throw new NotImplementedException("Db operator not implemented: " + dbConfig.DbName);
            }
        }
    }
}
