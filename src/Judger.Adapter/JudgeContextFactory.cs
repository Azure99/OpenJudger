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
    /// 评测任务工厂
    /// </summary>
    public static class JudgeContextFactory
    {
        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 创建JudgeTask实例
        /// </summary>
        /// <param name="submitId">提交ID</param>
        /// <param name="problemId">问题ID</param>
        /// <param name="dataVersion">测试数据版本</param>
        /// <param name="language">语言</param>
        /// <param name="sourceCode">源代码</param>
        /// <param name="author">作者</param>
        /// <param name="timeLimit">时间限制</param>
        /// <param name="memoryLimit">内存限制</param>
        /// <param name="judgeAllCases">是否评测全部样例(即使遇到错误答案)</param>
        /// <param name="specialJudge">是否为SpecialJudge</param>
        /// <param name="dbJudge">是否为数据库评测</param>
        /// <returns>JudgeTask实例</returns>
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
        /// 创建JudgeTask实例
        /// </summary>
        /// <param name="submitId">提交ID</param>
        /// <param name="problemId">问题ID</param>
        /// <param name="dataVersion">测试数据版本</param>
        /// <param name="language">语言</param>
        /// <param name="sourceCode">源代码</param>
        /// <param name="author">作者</param>
        /// <param name="timeLimit">时间限制</param>
        /// <param name="memoryLimit">内存限制</param>
        /// <param name="judgeAllCases">是否评测全部样例(即使遇到错误答案)</param>
        /// <param name="judgeType">评测类型</param>
        /// <returns>JudgeTask实例</returns>
        public static JudgeContext Create(string submitId, string problemId, string dataVersion,
            string language, string sourceCode, string author = "",
            int timeLimit = 1000, int memoryLimit = 262144, bool judgeAllCases = false,
            JudgeType judgeType = JudgeType.ProgramJudge)
        {
            ILangConfig langConfig = ConfigManager.GetLanguageConfig(language);

            if (langConfig == null)
                throw new JudgeException("Unsupported language: " + language);

            // 分配评测临时目录
            string tempDirectory = RandomString.Next(16);
            if (langConfig is ProgramLangConfig langCfg)
            {
                tempDirectory = GetTempDirectory(langCfg.JudgeDirectory);
                UpdatePathInfo(langCfg, tempDirectory);
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

        /// <summary>
        /// 更新语言配置中的路径信息
        /// </summary>
        private static void UpdatePathInfo(ProgramLangConfig langConfig, string tempDirectory)
        {
            string appDirectory = PathHelper.GetBaseAbsolutePath("");

            if (!Directory.Exists(tempDirectory))
                Directory.CreateDirectory(tempDirectory);

            // 替换<tempdir>字段
            langConfig.CompilerPath = ReplacePathInfo(langConfig.CompilerPath, tempDirectory, appDirectory);
            langConfig.CompilerWorkDirectory =
                ReplacePathInfo(langConfig.CompilerWorkDirectory, tempDirectory, appDirectory);
            langConfig.CompilerArgs = ReplacePathInfo(langConfig.CompilerArgs, tempDirectory, appDirectory);
            langConfig.RunnerPath = ReplacePathInfo(langConfig.RunnerPath, tempDirectory, appDirectory);
            langConfig.RunnerWorkDirectory =
                ReplacePathInfo(langConfig.RunnerWorkDirectory, tempDirectory, appDirectory);
            langConfig.RunnerArgs = ReplacePathInfo(langConfig.RunnerArgs, tempDirectory, appDirectory);
        }

        /// <summary>
        /// 在语言的评测目录下分配临时评测目录
        /// </summary>
        /// <param name="judgeDir">语言的评测目录</param>
        private static string GetTempDirectory(string judgeDir)
        {
            return Path.Combine(
                       PathHelper.GetBaseAbsolutePath(judgeDir),
                       RandomString.Next(32)) + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// 替换路径信息
        /// </summary>
        private static string ReplacePathInfo(string path, string tempDir, string appDir)
        {
            return path.Replace("<tempdir>", tempDir).Replace("<appdir>", appDir);
        }

        /// <summary>
        /// 获取时间补偿系数
        /// </summary>
        private static double GetTimeCompensation(ILangConfig langConfig)
        {
            if (langConfig is ProgramLangConfig config)
                return config.TimeCompensation;

            return 1;
        }
    }
}