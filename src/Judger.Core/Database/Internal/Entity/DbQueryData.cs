using System.Collections.Generic;
using Newtonsoft.Json;

namespace Judger.Core.Database.Internal.Entity
{
    /// <summary>
    /// 数据库查询数据
    /// </summary>
    public class DbQueryData
    {
        public string Name { get; set; }

        public string[] FieldNames { get; set; }

        [JsonIgnore]
        public int FieldCount => FieldNames.Length;

        public List<string[]> Records { get; set; }
    }
}