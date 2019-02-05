using System;
using Judger.Entity;

namespace Judger.Core
{
    /// <summary>
    /// Judger基类
    /// </summary>
    public abstract class BaseJudger : IDisposable
    {
        protected JudgeTask JudgeTask { get; }
        public BaseJudger(JudgeTask task)
        {
            JudgeTask = task;
        }

        /// <summary>
        /// 评测此任务
        /// </summary>
        /// <returns>评测结果s</returns>
        public abstract JudgeResult Judge();

        public virtual void Dispose()
        { }
    }
}
