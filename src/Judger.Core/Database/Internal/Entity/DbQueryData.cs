using System.Collections.Generic;
using Newtonsoft.Json;

namespace Judger.Core.Database.Internal.Entity
{
    /// <summary>
    /// 数据库查询数据
    /// </summary>
    public class DbQueryData
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 字段名
        /// </summary>
        public string[] FieldNames { get; set; }

        /// <summary>
        /// 字段数
        /// </summary>
        [JsonIgnore]
        public int FieldCount
        {
            get { return FieldNames.Length; }
        }

        /// <summary>
        /// 记录
        /// </summary>
        public List<string[]> Records { get; set; }
    }
}