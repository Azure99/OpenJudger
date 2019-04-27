using System;
using Newtonsoft.Json;

namespace Judger.Entity.Database
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    [Serializable]
    public class DbLangConfig : ILangConfig
    {
        /// <summary>
        /// 数据库管理系统名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 是否为数据库配置
        /// </summary>
        public bool IsDbConfig { get; set; } = true;

        /// <summary>
        /// 对查询字段是否大小写敏感
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// 数据库驱动路径
        /// </summary>
        public string DriverPath { get; set; } = "";

        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; } = "";

        /// <summary>
        /// 数据库名
        /// </summary>
        public string Database { get; set; } = "";

        /// <summary>
        /// 用户名
        /// </summary>
        public string User { get; set; } = "";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// 连接字符串模板
        /// </summary>
        public string ConnStringTemplate { get; set; } = "";

        /// <summary>
        /// 数据库的连接字符串
        /// </summary>
        [JsonIgnore]
        public string ConnectionString
        {
            get
            {
                string connString = ConnStringTemplate;
                connString = connString.Replace("<Server>", Server);
                connString = connString.Replace("<Database>", Database);
                connString = connString.Replace("<User>", User);
                connString = connString.Replace("<Password>", Password);

                return connString;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}