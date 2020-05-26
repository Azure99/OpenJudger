using System;
using Judger.Models;

namespace Judger.Adapter
{
    /// <summary>
    /// TaskSubmitter接口
    /// </summary>
    /// 用于向后端提交评测结果
    public interface ITaskSubmitter : IDisposable
    {
        /// <summary>
        /// 向后端提交评测结果
        /// </summary>
        /// <param name="context">评测上下文</param>
        /// <returns>是否成功</returns>
        bool Submit(JudgeContext context);
    }
}