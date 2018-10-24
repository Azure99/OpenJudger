using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;

namespace Judger.Judger.Models
{
    /// <summary>
    /// 单例Judge结果
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
