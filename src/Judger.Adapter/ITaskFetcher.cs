using System;
using Judger.Models;

namespace Judger.Adapter
{
    /// <summary>
    /// TaskFetcher接口
    /// </summary>
    /// 用于从后端取回评测任务
    public interface ITaskFetcher : IDisposable
    {
        /// <summary>
        /// 从后端取回任务并创建评测上下文
        /// 可返回多个任务的上下文, 如果没有任务, 返回空数组
        /// </summary>
        JudgeContext[] Fetch();
    }
}