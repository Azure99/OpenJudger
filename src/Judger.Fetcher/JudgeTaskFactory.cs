using System.IO;
using Judger.Entity;
using Judger.Entity.Program;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher
{
    /// <summary>
    /// 评测任务工厂
    /// </summary>
    public static class JudgeTaskFactory
    {
        
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
        public static JudgeTask Create(int submitId, int problemId, string dataVersion, string language, string sourceCode,
                                       string author = "", int timeLimit = 1000, int memoryLimit = 262144, 
                                       bool judgeAllCases = false, bool specialJudge = false, bool dbJudge = false)
        {
            JudgeType judgeType = JudgeType.ProgramJudge;
            if (dbJudge)
            {
                judgeType = JudgeType.DbJudge;
            }
            else if (specialJudge)
            {
                judgeType = JudgeType.SpecialJudge;
            }
            
            return Create(submitId, problemId, dataVersion, language, sourceCode, author, timeLimit, memoryLimit, judgeAllCases, judgeType);
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
        public static JudgeTask Create(int submitId, int problemId, string dataVersion, string language, string sourceCode,
                                       string author = "", int timeLimit = 1000, int memoryLimit = 262144, 
                                       bool judgeAllCases = false, JudgeType judgeType = JudgeType.ProgramJudge)
        {
            ILangConfig langConfig = ConfigManager.GetLanguageConfig(language);

            // 分配评测临时目录
            string tempDirectory = RandomString.Next(16);
            
            if (langConfig is ProgramLangConfig)
            {
                ProgramLangConfig langcfg = langConfig as ProgramLangConfig;

                tempDirectory = GetTempDirectory(langcfg.JudgeDirectory);
                if (!Directory.Exists(tempDirectory))
                {
                    Directory.CreateDirectory(tempDirectory);
                    // 替换<tempdir>字段
                    langcfg.CompilerPath = langcfg.CompilerPath.Replace("<tempdir>", tempDirectory);
                    langcfg.CompilerWorkDirectory = langcfg.CompilerWorkDirectory.Replace("<tempdir>", tempDirectory);
                    langcfg.CompilerArgs = langcfg.CompilerArgs.Replace("<tempdir>", tempDirectory);
                    langcfg.RunnerPath = langcfg.RunnerPath.Replace("<tempdir>", tempDirectory);
                    langcfg.RunnerWorkDirectory = langcfg.RunnerWorkDirectory.Replace("<tempdir>", tempDirectory);
                    langcfg.RunnerArgs = langcfg.RunnerArgs.Replace("<tempdir>", tempDirectory);

                    // 使用绝对路径
                    langcfg.CompilerPath = PathHelper.GetBaseAbsolutePath(langcfg.CompilerPath);
                    langcfg.CompilerWorkDirectory = PathHelper.GetBaseAbsolutePath(langcfg.CompilerWorkDirectory);
                    langcfg.RunnerWorkDirectory = PathHelper.GetBaseAbsolutePath(langcfg.RunnerWorkDirectory);
                    langcfg.RunnerPath = PathHelper.GetBaseAbsolutePath(langcfg.RunnerPath);
                }
            }

            double timeCompensation = langConfig is ProgramLangConfig ? 
                                      (langConfig as ProgramLangConfig).TimeCompensation :
                                      1;

            JudgeTask task = new JudgeTask
            {
                SubmitId = submitId,
                ProblemId = problemId,
                DataVersion = dataVersion,
                Language = language,
                SourceCode = sourceCode,
                Author = author,
                TimeLimit = (int)(timeLimit / timeCompensation),
                MemoryLimit = memoryLimit,
                JudgeAllCases = judgeAllCases,
                JudgeType = judgeType,
                LangConfig = langConfig,
                TempJudgeDirectory = tempDirectory
            };

            return task;
        }

        private static string GetTempDirectory(string judgeDir)
        {
            return Path.Combine(
                PathHelper.GetBaseAbsolutePath(judgeDir), 
                RandomString.Next(32)) + Path.DirectorySeparatorChar;
        }
    }
}
