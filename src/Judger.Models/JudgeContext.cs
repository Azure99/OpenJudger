using System;
using System.Collections.Generic;
using Judger.Models.Judge;

namespace Judger.Models
{
    /// <summary>
    /// 评测上下文
    /// </summary>
    public class JudgeContext : ICloneable
    {
        public JudgeContext(JudgeTask task, JudgeResult result, string tempDirectory, ILangConfig langConfig)
        {
            Task = task;
            TempDirectory = tempDirectory;
            LangConfig = langConfig;
            Result = result;
        }

        /// <summary>
        /// 评测任务
        /// </summary>
        public JudgeTask Task { get; set; }

        /// <summary>
        /// 判题所用临时目录
        /// </summary>
        public string TempDirectory { get; set; }

        /// <summary>
        /// 编程语言配置信息
        /// </summary>
        public ILangConfig LangConfig { get; set; }

        /// <summary>
        /// 评测结果
        /// </summary>
        public JudgeResult Result { get; set; }

        /// <summary>
        /// 附加配置信息
        /// </summary>
        public Dictionary<string, object> AdditionalInfo { get; set; } = new Dictionary<string, object>();

        public object Clone()
        {
            JudgeContext context = (JudgeContext) MemberwiseClone();
            context.Task = Task.Clone() as JudgeTask;
            context.LangConfig = LangConfig.Clone() as ILangConfig;
            context.Result = Result.Clone() as JudgeResult;

            return context;
        }
    }
}