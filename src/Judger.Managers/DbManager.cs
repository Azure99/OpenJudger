using System;
using System.Collections.Generic;
using System.Text;
using Judger.Entity;

namespace Judger.Managers
{
    public static class DbManager
    {
        private static Configuration _config = ConfigManager.Config;
        private static Dictionary<string, DbLangConfig> _dbDic = new Dictionary<string, DbLangConfig>();

        static DbManager()
        {
            foreach(var dbConf in _config.Databases)
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
