using System;

namespace Judger.Models.Judge
{
    /// <summary>
    /// 判题结果
    /// </summary>
    public class JudgeResult : ICloneable
    {
        /// <summary>
        /// 提交ID
        /// </summary>
        public string SubmitId { get; set; }

        /// <summary>
        /// 问题ID
        /// </summary>
        public string ProblemId { get; set; }

        /// <summary>
        /// 提交者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 判题结果码
        /// </summary>
        public JudgeResultCode ResultCode { get; set; }

        /// <summary>
        /// 判题详情(用于获取错误详情)
        /// </summary>
        public string JudgeDetail { get; set; }

        /// <summary>
        /// 通过率, 标识通过了几组数据
        /// </summary>
        public double PassRate { get; set; }

        /// <summary>
        /// 时间消耗
        /// </summary>
        public int TimeCost { get; set; }

        /// <summary>
        /// 内存消耗
        /// </summary>
        public int MemoryCost { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}