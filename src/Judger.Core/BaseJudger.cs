using System;
using Judger.Models;

namespace Judger.Core
{
    /// <summary>
    /// Judger基类
    /// </summary>
    public abstract class BaseJudger : IDisposable
    {
        public BaseJudger(JudgeContext context)
        {
            Context = context;
            JudgeTask = context.Task;
            JudgeResult = context.Result;
        }

        protected JudgeContext Context { get; }
        protected JudgeTask JudgeTask { get; }
        protected JudgeResult JudgeResult { get; }

        public virtual void Dispose()
        { }

        /// <summary>
        /// 评测此任务
        /// </summary>
        /// <returns>评测结果s</returns>
        public abstract void Judge();
    }
}