using System;
using Judger.Models;

namespace Judger.Adapter
{
    /// <summary>
    /// JudgeResult提交接口
    /// </summary>
    public interface ITaskSubmitter : IDisposable
    {
        /// <summary>
        /// 提交评测结果
        /// </summary>
        /// <param name="context">JudgeContext</param>
        /// <returns>是否成功</returns>
        bool Submit(JudgeContext context);
    }
}