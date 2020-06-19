using System;
using Judger.Models;

namespace Judger.Adapter
{
    /// <summary>
    /// TaskSubmitter接口
    /// </summary>
    /// 用于向服务端提交评测结果
    public interface ITaskSubmitter : IDisposable
    {
        /// <summary>
        /// 提交评测结果
        /// </summary>
        /// <param name="context">评测上下文</param>
        /// <returns>是否成功</returns>
        /// 此方法在评测完成后回调, 以向服务端提交结果
        /// 评测结果可从JudgeContext.Result中获取
        void Submit(JudgeContext context);
    }
}