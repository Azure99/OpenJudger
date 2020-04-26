﻿using Judger.Core.Database;
using Judger.Core.Program;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;

namespace Judger.Core
{
    public static class JudgerFactory
    {
        /// <summary>
        /// 根据上下文创建Judger实例
        /// </summary>
        /// <param name="context">JudgeContext</param>
        /// <returns>Judger实例</returns>
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