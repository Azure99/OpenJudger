using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Judger.Entity
{
    public interface ILangConfig : ICloneable
    {
        /// <summary>
        /// 语言名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 是否为数据库的配置, true为数据库配置, false为程序配置
        /// </summary>
        [JsonIgnore]
        bool IsDbConfig { get; set; }
    }
}
