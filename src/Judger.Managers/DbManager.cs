using System;
using System.Collections.Generic;
using System.Text;
using Judger.Entity;

namespace Judger.Managers
{
    public static class DbManager
    {
        private static Configuration _config = ConfigManager.Config;
        private static Dictionary<string, DbConfiguration> _dbDic = new Dictionary<string, DbConfiguration>();

        static DbManager()
        {
            foreach(var dbConf in _config.Databases)
            {
                if (!_dbDic.ContainsKey(dbConf.DbName))  
                {
                    _dbDic.Add(dbConf.DbName, dbConf);
                }
            }
        }

        /// <summary>
        /// 获取数据库配置
        /// </summary>
        /// <param name="name">数据库名称</param>
        /// <returns>数据库配置</returns>
        public static DbConfiguration GetDbConfiguration(string name)
        {
            return _dbDic[name].Clone() as DbConfiguration;
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
