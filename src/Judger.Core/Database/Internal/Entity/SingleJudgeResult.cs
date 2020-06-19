using Judger.Models.Judge;

namespace Judger.Core.Database.Internal.Entity
{
    /// <summary>
    /// 单组用例测试结果
    /// </summary>
    public class SingleJudgeResult
    {
        public JudgeResultCode ResultCode { get; set; }

        public string JudgeDetail { get; set; }

        public int TimeCost { get; set; }
    }
}