using System;
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
        public static DbLangConfig GetDbConfiguration(string name)
        {
            return DbDic[name].Clone() as DbLangConfig;
        }

        /// <summary>
        /// 获取数据库类型
        /// </summary>
        public static DatabaseType GetDatabaseType(string name)
        {
            return Enum.Parse<DatabaseType>(name);
        }
    }
}