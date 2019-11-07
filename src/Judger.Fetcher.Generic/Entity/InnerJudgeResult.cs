using Judger.Models.Judge;
using Newtonsoft.Json;

namespace Judger.Fetcher.Generic.Entity
{
    /// <summary>
    /// 内部判题结果
    /// </summary>
    public class InnerJudgeResult
    {
        /// <summary>
        /// 提交Id
        /// </summary>
        [JsonProperty(PropertyName = "submitId")]
        public string SubmitId { get; set; }

        /// <summary>
        /// 判题结果码
        /// </summary>
        [JsonProperty(PropertyName = "resultCode")]
        public JudgeResultCode ResultCode { get; set; }

        /// <summary>
        /// 判题详情(包含错误详情)
        /// </summary>
        [JsonProperty(PropertyName = "judgeDetail")]
        public string JudgeDetail { get; set; }

        /// <summary>
        /// 通过率, 标识通过了几组数据
        /// </summary>
        [JsonProperty(PropertyName = "passRate")]
        public double PassRate { get; set; }

        /// <summary>
        /// 时间消耗
        /// </summary>
        [JsonProperty(PropertyName = "timeCost")]
        public int TimeCost { get; set; }

        /// <summary>
        /// 内存消耗
        /// </summary>
        [JsonProperty(PropertyName = "memoryCost")]
        public int MemoryCost { get; set; }
    }
}