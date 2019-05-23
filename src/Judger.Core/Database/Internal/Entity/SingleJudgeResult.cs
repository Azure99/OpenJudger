using Judger.Models;
using Judger.Models.Judge;

namespace Judger.Core.Database.Internal.Entity
{
    /// <summary>
    /// 单组用例测试结果
    /// </summary>
    public class SingleJudgeResult
    {
        /// <summary>
        /// 结果码
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
    }
}