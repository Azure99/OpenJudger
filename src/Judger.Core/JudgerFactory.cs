using Judger.Entity;
using Judger.Core.Program;
using Judger.Core.Database;

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
            switch (task.JudgeType)
            {
                case JudgeType.ProgramJudge: return new ProgramJudger(task);
                case JudgeType.SpecialJudge: return new SpecialJudger(task);
                case JudgeType.DbJudge: return new DbJudger(task);
                default: throw new JudgeException("Unknown JudgeType!");
            }
        }
    }
}
