using System;
using Judger.Models;
using Judger.Models.Judge;

namespace Judger.Core
{
    public abstract class BaseJudger : IDisposable
    {
        protected BaseJudger(JudgeContext context)
        {
            Context = context;
            JudgeTask = context.Task;
            JudgeResult = context.Result;
        }

        protected JudgeContext Context { get; }
        protected JudgeTask JudgeTask { get; }
        protected JudgeResult JudgeResult { get; }

        public virtual void Dispose()
        {
        }

        public abstract void Judge();
    }
}