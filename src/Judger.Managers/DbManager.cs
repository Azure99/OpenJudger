using System;
using System.Collections.Generic;
using Judger.Models;
using Judger.Models.Database;

namespace Judger.Managers
{
    /// <summary>
    /// 数据库评测管理器
    /// </summary>
    public static class DbManager
    {
        private static readonly Dictionary<string, DbLangConfig> DbConfigDic = new Dictionary<string, DbLangConfig>();

        static DbManager()
        {
            foreach (DbLangConfig config in Config.Databases)
            {
                if (!DbConfigDic.ContainsKey(config.Name))
                    DbConfigDic.Add(config.Name, config);
            }
        }

        private static Configuration Config { get; } = ConfigManager.Config;

        public static DbLangConfig GetDbConfig(string name)
        {
            return DbConfigDic[name].Clone() as DbLangConfig;
        }

        public static DatabaseType GetDatabaseType(string name)
        {
            return Enum.Parse<DatabaseType>(name);
        }
    }
}