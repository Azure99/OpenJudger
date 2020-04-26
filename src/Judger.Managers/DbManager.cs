﻿using System;
using System.Collections.Generic;
using Judger.Models;
using Judger.Models.Database;

namespace Judger.Managers
{
    /// <summary>
    /// Database Judge管理器
    /// </summary>
    public static class DbManager
    {
        /// <summary>
        /// 数据库配置字典
        /// </summary>
        private static readonly Dictionary<string, DbLangConfig> DbDic = new Dictionary<string, DbLangConfig>();

        static DbManager()
        {
            foreach (DbLangConfig dbConf in Config.Databases)
            {
                if (!DbDic.ContainsKey(dbConf.Name))
                    DbDic.Add(dbConf.Name, dbConf);
            }
        }

        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 获取数据库配置
        /// </summary>
        /// <param name="name">数据库名称</param>
        /// <returns>数据库配置</returns>
        public static DbLangConfig GetDbConfiguration(string name)
        {
            return DbDic[name].Clone() as DbLangConfig;
        }

        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <param name="name">数据库名称</param>
        /// <returns>数据库类型</returns>
        public static DatabaseType GetDatabaseType(string name)
        {
            return Enum.Parse<DatabaseType>(name);
        }
    }
}