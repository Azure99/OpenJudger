using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Judger.Entity
{
    /// <summary>
    /// 数据库配置
    /// </summary>
    [Serializable]
    public class DbConfiguration : ICloneable
    {
        /// <summary>
        /// 数据库管理系统名称
        /// </summary>
        public string DbName { get; set; } = DatabaseType.mysql.ToString();

        /// <summary>
        /// 数据库驱动路径
        /// </summary>
        public string DriverPath { get; set; } = "Pomelo.Data.MySql.dll";

        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; } = "localhost";

        /// <summary>
        /// 数据库名
        /// </summary>
        public string Database { get; set; } = "judger";

        /// <summary>
        /// 用户名
        /// </summary>
        public string User { get; set; } = "root";

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; } = "123456";

        /// <summary>
        /// 连接字符串模板
        /// </summary>
        public string ConnStringTemplate { get; set; } = "Server=<Server>;Database=<Database>;User=<User>;Password=<Password>;CharSet=utf8;";

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
            return this.MemberwiseClone();
        }
    }
}
