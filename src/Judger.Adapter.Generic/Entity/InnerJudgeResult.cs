using Judger.Models.Judge;
using Newtonsoft.Json;

namespace Judger.Adapter.Generic.Entity
{
    public class InnerJudgeResult
    {
        [JsonProperty(PropertyName = "submitId")]
        public string SubmitId { get; set; }

        [JsonProperty(PropertyName = "resultCode")]
        public JudgeResultCode ResultCode { get; set; }

        [JsonProperty(PropertyName = "judgeDetail")]
        public string JudgeDetail { get; set; }

        [JsonProperty(PropertyName = "passRate")]
        public double PassRate { get; set; }

        [JsonProperty(PropertyName = "timeCost")]
        public int TimeCost { get; set; }

        [JsonProperty(PropertyName = "memoryCost")]
        public int MemoryCost { get; set; }
    }
}