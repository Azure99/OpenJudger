using System;
using Newtonsoft.Json;

namespace Judger.Models.Database
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    [Serializable]
    public class DbLangConfig : ILangConfig
    {
        /// <summary>
        /// 数据库管理系统(DBMS)名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 对查询字段是否大小写敏感
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// 数据库驱动路径
        /// </summary>
        public string DriverPath { get; set; } = "";

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Server { get; set; } = "";

        /// <summary>
        /// 数据库名
        /// </summary>
        public string Database { get; set; } = "";

        /// <summary>
        /// 数据库用户名
        /// </summary>
        public string User { get; set; } = "";

        /// <summary>
        /// 数据库密码
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
        public string ConnectionString => ConnStringTemplate
            .Replace("<Server>", Server)
            .Replace("<Database>", Database)
            .Replace("<User>", User)
            .Replace("<Password>", Password);

        /// <summary>
        /// 是否为数据库配置
        /// </summary>
        public bool IsDbConfig { get; set; } = true;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}