using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;
using Judger.Utils;

namespace Judger.Managers
{
    /// <summary>
    /// 评测任务工厂
    /// </summary>
    public static class JudgeTaskFactory
    {
        /// <summary>
        /// 创建JudgeTask实例
        /// </summary>
        /// <param name="submitID">提交ID</param>
        /// <param name="problemID">问题ID</param>
        /// <param name="dataVersion">测试数据版本</param>
        /// <param name="language">语言</param>
        /// <param name="sourceCode">源代码</param>
        /// <param name="author">作者</param>
        /// <param name="timeLimit">时间限制</param>
        /// <param name="memoryLimit">内存限制</param>
        /// <param name="specialJudge">是否为SpecialJudge</param>
        /// <returns>JudgeTask实例</returns>
        public static JudgeTask Create(int submitID, int problemID, string dataVersion, string language, string sourceCode,
                                             string author = "", int timeLimit = 1000, int memoryLimit = 262144, bool specialJudge = false)
        {
            LanguageConfiguration langConfig = ConfigManager.GetLanguageConfig(language).Clone() as LanguageConfiguration;

            // 分配评测临时目录
            string tempDirectory = GetTempDirectory(langConfig.JudgeDirectory);

            // 替换<tempdir>字段
            langConfig.CompilerPath = langConfig.CompilerPath.Replace("<tempdir>", tempDirectory);
            langConfig.CompilerWorkDirectory = langConfig.CompilerWorkDirectory.Replace("<tempdir>", tempDirectory);
            langConfig.CompilerArgs = langConfig.CompilerArgs.Replace("<tempdir>", tempDirectory);
            langConfig.RunnerPath = langConfig.RunnerPath.Replace("<tempdir>", tempDirectory);
            langConfig.RunnerWorkDirectory = langConfig.RunnerWorkDirectory.Replace("<tempdir>", tempDirectory);
            langConfig.RunnerArgs = langConfig.RunnerArgs.Replace("<tempdir>", tempDirectory);

            // 使用绝对路径
            langConfig.CompilerPath = PathHelper.GetBaseAbsolutePath(langConfig.CompilerPath);
            langConfig.CompilerWorkDirectory = PathHelper.GetBaseAbsolutePath(langConfig.CompilerWorkDirectory);
            langConfig.RunnerWorkDirectory = PathHelper.GetBaseAbsolutePath(langConfig.RunnerWorkDirectory);
            langConfig.RunnerPath = PathHelper.GetBaseAbsolutePath(langConfig.RunnerPath);

            JudgeTask task = new JudgeTask
            {
                SubmitID = submitID,
                ProblemID = problemID,
                DataVersion = dataVersion,
                Language = language,
                SourceCode = sourceCode,
                Author = author,
                TimeLimit = (int)(timeLimit / langConfig.TimeCompensation),
                MemoryLimit = memoryLimit,
                SpecialJudge = specialJudge,
                LangConfig = langConfig,
                TempJudgeDirectory = tempDirectory
            };

            return task;
        }

        private static string GetTempDirectory(string judgeDir)
        {
            return System.IO.Path.Combine(
                PathHelper.GetBaseAbsolutePath(judgeDir), 
                RandomString.Next(32)) + System.IO.Path.DirectorySeparatorChar;
        }
    }
}
