using Judger.Core.Database;
using Judger.Core.Program;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;

namespace Judger.Core
{
    /// <summary>
    /// Judger工厂
    /// </summary>
    /// 根据评测上下文创建对应Judger实现类的实例
    public static class JudgerFactory
    {
        /// <summary>
        /// 根据上下文创建Judger实例
        /// </summary>
        public static BaseJudger Create(JudgeContext context)
        {
            switch (context.Task.JudgeType)
            {
                case JudgeType.ProgramJudge:
                    return new ProgramJudger(context);
                case JudgeType.SpecialJudge:
                    return new SpecialJudger(context);
                case JudgeType.DbJudge:
                    return new DbJudger(context);
                default:
                    throw new JudgeException("Unknown JudgeType!");
            }
        }
    }
}