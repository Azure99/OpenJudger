using Judger.Models.Judge;
using Newtonsoft.Json;

namespace Judger.Adapter.Generic.Entity
{
    /// <summary>
    /// 内部临时评测任务
    /// </summary>
    public class InnerJudgeTask
    {
        /// <summary>
        /// 提交Id
        /// </summary>
        [JsonProperty(PropertyName = "submitId")]
        public string SubmitId { get; set; }

        /// <summary>
        /// 题目Id
        /// </summary>
        [JsonProperty(PropertyName = "problemId")]
        public string ProblemId { get; set; }

        /// <summary>
        /// 测试数据版本号
        /// </summary>
        [JsonProperty(PropertyName = "dataVersion")]
        public string DataVersion { get; set; } = "";

        /// <summary>
        /// 提交者
        /// </summary>
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; } = "";

        /// <summary>
        /// 编程语言
        /// </summary>
        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; } = "";

        /// <summary>
        /// 时间限制
        /// </summary>
        [JsonProperty(PropertyName = "timeLimit")]
        public int TimeLimit { get; set; } = 1000;

        /// <summary>
        /// 内存限制
        /// </summary>
        [JsonProperty(PropertyName = "memoryLimit")]
        public int MemoryLimit { get; set; } = 262144;

        /// <summary>
        /// 是否评测所有测试用例
        /// </summary>
        [JsonProperty(PropertyName = "judgeAllCases")]
        public bool JudgeAllCases { get; set; }

        /// <summary>
        /// 评测类型
        /// </summary>
        [JsonProperty(PropertyName = "judgeType")]
        public JudgeType JudgeType { get; set; } = JudgeType.ProgramJudge;

        /// <summary>
        /// 源代码
        /// </summary>
        [JsonProperty(PropertyName = "sourceCode")]
        public string SourceCode { get; set; } = "";
    }
}