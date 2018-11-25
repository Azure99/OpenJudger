using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;
using Judger.Judger.Models;

namespace Judger.Judger
{
    /// <summary>
    /// 单例Judger接口
    /// </summary>
    public interface ISingleJudger
    {
        /// <summary>
        /// 评测任务
        /// </summary>
        JudgeTask JudgeTask { get; set; }

        /// <summary>
        /// 评测
        /// </summary>
        /// <param name="input">答案输入</param>
        /// <param name="output">答案输出</param>
        /// <returns>单组评测结果</returns>
        SingleJudgeResult Judge(string input, string output);
    }
}
