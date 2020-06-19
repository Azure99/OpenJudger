using Judger.Models.Judge;
using Newtonsoft.Json;

namespace Judger.Adapter.Generic.Entity
{
    public class InnerJudgeTask
    {
        [JsonProperty(PropertyName = "submitId")]
        public string SubmitId { get; set; }

        [JsonProperty(PropertyName = "problemId")]
        public string ProblemId { get; set; }

        [JsonProperty(PropertyName = "dataVersion")]
        public string DataVersion { get; set; } = "";

        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; } = "";

        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; } = "";

        [JsonProperty(PropertyName = "timeLimit")]
        public int TimeLimit { get; set; } = 1000;

        [JsonProperty(PropertyName = "memoryLimit")]
        public int MemoryLimit { get; set; } = 262144;

        [JsonProperty(PropertyName = "judgeAllCases")]
        public bool JudgeAllCases { get; set; }

        [JsonProperty(PropertyName = "judgeType")]
        public JudgeType JudgeType { get; set; } = JudgeType.ProgramJudge;

        [JsonProperty(PropertyName = "sourceCode")]
        public string SourceCode { get; set; } = "";
    }
}