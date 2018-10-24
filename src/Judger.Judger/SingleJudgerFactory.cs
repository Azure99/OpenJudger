using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;

namespace Judger.Judger
{
    /// <summary>
    /// 单例Judger工厂
    /// </summary>
    public static class SingleJudgerFactory
    {
        /// <summary>
        /// 创建单例Judger
        /// </summary>
        public static ISingleJudger Create(JudgeTask task)
        {
            LanguageConfiguration config = task.LangConfig;

            ISingleJudger judger = new SingleJudger(config.RunnerPath)
            {
                RunnerWorkDirectory = config.RunnerWorkDirectory,
                RunnerArgs = config.RunnerArgs,
                TimeLimit = task.TimeLimit,
                MemoryLimit = task.MemoryLimit,
                OutputLimit = config.OutputLimit
            };

            return judger;
        }
    }
}
