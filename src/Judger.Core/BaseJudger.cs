using System;
using Judger.Models;

namespace Judger.Core
{
    /// <summary>
    /// Judger基类
    /// </summary>
    public abstract class BaseJudger : IDisposable
    {
        public BaseJudger(JudgeTask task)
        {
            JudgeTask = task;
        }

        protected JudgeTask JudgeTask { get; }

        public virtual void Dispose()
        { }

        /// <summary>
        /// 评测此任务
        /// </summary>
        /// <returns>评测结果s</returns>
        public abstract JudgeResult Judge();
    }
}