using System;
using Judger.Entity;

namespace Judger.Fetcher
{
    /// <summary>
    /// JudgeTask取回接口
    /// </summary>
    public interface ITaskFetcher : IDisposable
    {
        /// <summary>
        /// 从服务器取回JudgeTask
        /// </summary>
        /// <returns>JudgeTask数组</returns>
        JudgeTask[] Fetch();
    }
}
