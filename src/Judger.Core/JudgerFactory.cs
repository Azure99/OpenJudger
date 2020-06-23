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
        public static BaseJudger Create(JudgeContext context)
        {
            return context.Task.JudgeType switch
            {
                JudgeType.ProgramJudge => new ProgramJudger(context),
                JudgeType.SpecialJudge => new SpecialProgramJudger(context),
                JudgeType.DbJudge => new DbJudger(context),
                _ => throw new JudgeException("Unknown JudgeType!")
            };
        }
    }
}