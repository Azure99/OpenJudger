using System;
using Judger.Managers;
using Judger.Models.Database;

namespace Judger.Core.Database.Internal.DbOperator
{
    public static class DbOperatorFactory
    {
        /// <summary>
        /// 根据数据库管理系统(DBMS)的名称创建主操作器
        /// </summary>
        /// 主操作器权限为root, 负责新建库、创建新用户、分配权限等操作
        public static BaseDbOperator CreateMainOperatorByName(string dbmsName)
        {
            if (dbmsName != DatabaseType.mysql.ToString())
                throw new NotImplementedException("Db operator not implemented: " + dbmsName);

            DbLangConfig config = DbManager.GetDbConfig(dbmsName);
            return Create(config);
        }

        /// <summary>
        /// 创建数据库操作器
        /// </summary>
        /// 执行用户代码时应该使用由主操作器分配的无特权用户
        public static BaseDbOperator Create(DbLangConfig dbConfig)
        {
            string connString = dbConfig.ConnectionString;
            DbDriver driver = DbDriverLoader.Load(dbConfig.DriverPath);

            if (dbConfig.Name == DatabaseType.mysql.ToString())
                return new MySqlOperator(connString, driver);

            throw new NotImplementedException("Db operator not implemented: " + dbConfig.Name);
        }
    }
}