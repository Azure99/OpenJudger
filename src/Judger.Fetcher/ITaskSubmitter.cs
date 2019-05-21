using System;
using Judger.Models;

namespace Judger.Fetcher
{
    /// <summary>
    /// JudgeResult提交接口
    /// </summary>
    public interface ITaskSubmitter : IDisposable
    {
        /// <summary>
        /// 提交评测结果
        /// </summary>
        /// <param name="result">JudgeResult</param>
        /// <returns>是否成功</returns>
        bool Submit(JudgeResult result);
    }
}