using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Judger.Fetcher.SDNUOJ.Entity
{
    [Serializable]
    public class SDNUOJTaskEntity
    {
        [JsonProperty(PropertyName = "sid")]
        public string SubmitID { get; set; }

        [JsonProperty(PropertyName = "pid")]
        public string ProblemID { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Author { get; set; }

        [JsonProperty(PropertyName = "dataversion")]
        public string DataVersion { get; set; }

        [JsonProperty(PropertyName = "timelimit")]
        public string TimeLimit { get; set; }

        [JsonProperty(PropertyName = "memorylimit")]
        public string MemoryLimit { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "sourcecode")]
        public string SourceCode { get; set; }
    }
}
