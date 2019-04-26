﻿using System;
using Judger.Entity.Database;
using Judger.Managers;

namespace Judger.Core.Database.Internal.DbOperator
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
            if (dbName == DatabaseType.mysql.ToString())
            {
                DbLangConfig config = DbManager.GetDbConfiguration(dbName);
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
        public static BaseDbOperator Create(DbLangConfig dbConfig)
        {
            string connString = dbConfig.ConnectionString;
            DbDriver driver = DbDriverLoader.Load(dbConfig.DriverPath);

            if (dbConfig.Name == DatabaseType.mysql.ToString())
            {
                return new MySQL5xOperator(connString, driver);
            }
            else
            {
                throw new NotImplementedException("Db operator not implemented: " + dbConfig.Name);
            }
        }
    }
}