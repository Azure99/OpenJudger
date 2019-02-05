using System;
using Judger.Entity;
using Judger.Core.Program;

namespace Judger.Core
{
    /// <summary>
    /// Judger工厂
    /// </summary>
    public static class JudgerFactory
    {
        /// <summary>
        /// 创建Judger实例
        /// </summary>
        /// <param name="task">评测任务</param>
        /// <returns>Judger实例</returns>
        public static BaseJudger Create(JudgeTask task)
        {
            if(task.SpecialJudge)
            {
                throw new NotImplementedException();
            }
            else
            {
                return new ProgramJudger(task);
            }
        }
    }
}
