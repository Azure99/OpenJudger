using System;
using Judger.Models;

namespace Judger.Fetcher
{
    /// <summary>
    /// JudgeTask取回接口
    /// </summary>
    public interface ITaskFetcher : IDisposable
    {
        /// <summary>
        /// 从服务器取回评测任务
        /// </summary>
        /// <returns>JudgeContext数组</returns>
        JudgeContext[] Fetch();
    }
}