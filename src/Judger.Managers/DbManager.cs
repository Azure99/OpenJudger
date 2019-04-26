﻿using System;
using System.Collections.Generic;
using Judger.Entity;
using Judger.Entity.Database;

namespace Judger.Managers
{
    /// <summary>
    /// Database Judge管理器
    /// </summary>
    public static class DbManager
    {
        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 数据库配置字典
        /// </summary>
        private static Dictionary<string, DbLangConfig> _dbDic = new Dictionary<string, DbLangConfig>();

        static DbManager()
        {
            foreach (var dbConf in Config.Databases)
            {
                if (!_dbDic.ContainsKey(dbConf.Name))
                {
                    _dbDic.Add(dbConf.Name, dbConf);
                }
            }
        }

        /// <summary>
        /// 获取数据库配置
        /// </summary>
        /// <param name="name">数据库名称</param>
        /// <returns>数据库配置</returns>
        public static DbLangConfig GetDbConfiguration(string name)
        {
            return _dbDic[name].Clone() as DbLangConfig;
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