using System;
using Judger.Models;

namespace Judger.Adapter
{
    /// <summary>
    /// TaskFetcher接口
    /// </summary>
    /// 用于从服务端获取评测任务
    public interface ITaskFetcher : IDisposable
    {
        /// <summary>
        /// 尝试从服务端获取评测任务并创建对应的评测上下文
        /// </summary>
        /// <returns>评测上下文</returns>
        /// 此方法由JudgeService定期调用以实现任务轮询
        /// 可返回多个任务的上下文, 如果没有任务, 返回空数组
        JudgeContext[] Fetch();
    }
}