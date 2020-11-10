using System;
using Newtonsoft.Json;

namespace Judger.Models
{
    /// <summary>
    /// 语言配置接口
    /// </summary>
    public interface ILangConfig : ICloneable
    {
        /// <summary>
        /// 语言名称
        /// </summary>
        // ReSharper disable once UnusedMemberInSuper.Global
        string Name { get; set; }

        /// <summary>
        /// 是否为数据库的配置
        /// </summary>
        /// true为数据库配置, false为普通程序配置
        [JsonIgnore]
        // ReSharper disable once UnusedMemberInSuper.Global
        bool IsDbConfig { get; set; }
    }
}