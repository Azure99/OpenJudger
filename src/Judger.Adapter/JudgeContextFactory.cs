using System.IO;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Adapter
{
    /// <summary>
    /// JudgeContext工厂
    /// </summary>
    /// 根据指定参数创建评测上下文
    public static class JudgeContextFactory
    {
        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 创建评测上下文
        /// </summary>
        /// <param name="submitId">提交Id</param>
        /// <param name="problemId">问题Id</param>
        /// <param name="dataVersion">测试数据版本</param>
        /// <param name="language">语言</param>
        /// <param name="sourceCode">源代码</param>
        /// <param name="author">作者</param>
        /// <param name="timeLimit">时间限制</param>
        /// <param name="memoryLimit">内存限制</param>
        /// <param name="judgeAllCases">是否评测全部样例(即使遇到错误答案)</param>
        /// <param name="specialJudge">是否为SpecialJudge</param>
        /// <param name="dbJudge">是否为数据库评测</param>
        /// <returns>评测上下文</returns>
        public static JudgeContext Create(string submitId, string problemId, string dataVersion,
            string language, string sourceCode, string author = "",
            int timeLimit = 1000, int memoryLimit = 262144, bool judgeAllCases = false,
            bool specialJudge = false, bool dbJudge = false)
        {
            JudgeType judgeType = JudgeType.ProgramJudge;
            if (dbJudge)
                judgeType = JudgeType.DbJudge;
            else if (specialJudge)
                judgeType = JudgeType.SpecialJudge;

            return Create(
                submitId, problemId, dataVersion,
                language, sourceCode, author,
                timeLimit, memoryLimit,
                judgeAllCases, judgeType);
        }

        /// <summary>
        /// 创建评测上下文
        /// </summary>
        /// <param name="submitId">提交Id</param>
        /// <param name="problemId">问题Id</param>
        /// <param name="dataVersion">测试数据版本</param>
        /// <param name="language">语言</param>
        /// <param name="sourceCode">源代码</param>
        /// <param name="author">作者</param>
        /// <param name="timeLimit">时间限制</param>
        /// <param name="memoryLimit">内存限制</param>
        /// <param name="judgeAllCases">是否评测全部样例(即使遇到错误答案)</param>
        /// <param name="judgeType">评测类型</param>
        /// <returns>评测上下文</returns>
        public static JudgeContext Create(string submitId, string problemId, string dataVersion,
            string language, string sourceCode, string author = "",
            int timeLimit = 1000, int memoryLimit = 262144, bool judgeAllCases = false,
            JudgeType judgeType = JudgeType.ProgramJudge)
        {
            ILangConfig langConfig = ConfigManager.GetLanguageConfig(language);

            if (langConfig == null)
                throw new JudgeException("Unsupported language: " + language);

            string tempDirectory = RandomString.Next(16);
            if (langConfig is ProgramLangConfig langCfg)
            {
                tempDirectory = GetTempDirectory(langCfg.JudgeDirectory);
                UpdatePathFields(langCfg, tempDirectory);
            }

            double timeCompensation = GetTimeCompensation(langConfig);

            JudgeTask task = new JudgeTask
            {
                SubmitId = submitId,
                ProblemId = problemId,
                DataVersion = dataVersion,
                Language = language,
                SourceCode = sourceCode,
                Author = author,
                TimeLimit = (int) (timeLimit / timeCompensation),
                MemoryLimit = memoryLimit,
                JudgeAllCases = judgeAllCases,
                JudgeType = judgeType
            };

            JudgeResult result = new JudgeResult
            {
                SubmitId = task.SubmitId,
                ProblemId = task.ProblemId,
                Author = task.Author,
                JudgeDetail = "",
                MemoryCost = Config.MinimumMemoryCost,
                TimeCost = 0,
                PassRate = 0,
                ResultCode = JudgeResultCode.Accepted
            };

            return new JudgeContext(task, result, tempDirectory, langConfig);
        }

        private static void UpdatePathFields(ProgramLangConfig langConfig, string tempDirectory)
        {
            string appDirectory = PathHelper.GetBaseAbsolutePath("");

            if (!Directory.Exists(tempDirectory))
                Directory.CreateDirectory(tempDirectory);

            // 替换<tempdir>字段
            langConfig.CompilerPath = ReplacePathFields(langConfig.CompilerPath, tempDirectory, appDirectory);
            langConfig.CompilerWorkDirectory =
                ReplacePathFields(langConfig.CompilerWorkDirectory, tempDirectory, appDirectory);
            langConfig.CompilerArgs = ReplacePathFields(langConfig.CompilerArgs, tempDirectory, appDirectory);
            langConfig.RunnerPath = ReplacePathFields(langConfig.RunnerPath, tempDirectory, appDirectory);
            langConfig.RunnerWorkDirectory =
                ReplacePathFields(langConfig.RunnerWorkDirectory, tempDirectory, appDirectory);
            langConfig.RunnerArgs = ReplacePathFields(langConfig.RunnerArgs, tempDirectory, appDirectory);
        }

        private static string GetTempDirectory(string judgeDir)
        {
            return Path.Combine(
                PathHelper.GetBaseAbsolutePath(judgeDir),
                RandomString.Next(32)) + Path.DirectorySeparatorChar;
        }

        private static string ReplacePathFields(string path, string tempDir, string appDir)
        {
            return path.Replace("<tempdir>", tempDir).Replace("<appdir>", appDir);
        }

        private static double GetTimeCompensation(ILangConfig langConfig)
        {
            if (langConfig is ProgramLangConfig config)
                return config.TimeCompensation;

            return 1;
        }
    }
}