using Judger.Models.Judge;

namespace Judger.Core.Program.Internal.Entity
{
    /// <summary>
    /// 单组用例的评测结果
    /// </summary>
    public class SingleJudgeResult
    {
        /// <summary>
        /// 判题结果码
        /// </summary>
        public JudgeResultCode ResultCode { get; set; }

        /// <summary>
        /// 详细信息(用于返回错误信息)
        /// </summary>
        public string JudgeDetail { get; set; }

        /// <summary>
        /// 时间消耗
        /// </summary>
        public int TimeCost { get; set; }

        /// <summary>
        /// 内存消耗
        /// </summary>
        public int MemoryCost { get; set; }
    }
}