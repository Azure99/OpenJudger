using System;
using Newtonsoft.Json;

namespace Judger.Adapter.SDNUOJ.Entity
{
    [Serializable]
    public class SdnuojJudgeTask
    {
        [JsonProperty(PropertyName = "sid")]
        public string SubmitId { get; set; }

        [JsonProperty(PropertyName = "pid")]
        public string ProblemId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Author { get; set; }

        [JsonProperty(PropertyName = "dataversion")]
        public string DataVersion { get; set; }

        [JsonProperty(PropertyName = "timeliest")]
        public string TimeLimit { get; set; }

        [JsonProperty(PropertyName = "memorylimit")]
        public string MemoryLimit { get; set; }

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; }

        [JsonProperty(PropertyName = "dbjudge")]
        public string DbJudge { get; set; } = "false";

        [JsonProperty(PropertyName = "sourcecode")]
        public string SourceCode { get; set; }
    }
}