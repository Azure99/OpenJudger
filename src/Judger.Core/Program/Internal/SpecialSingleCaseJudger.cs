using System;
using System.Diagnostics;
using System.IO;
using Judger.Core.Program.Internal.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Core.Program.Internal
{
    public class SpecialSingleCaseJudger : BaseSingleCaseJudger
    {
        private const string SPJ_INPUT_NAME = "input.txt";
        private const string SPJ_OUTPUT_NAME = "output.txt";
        private const string SPJ_USER_OUTPUT_NAME = "user.txt";

        public SpecialSingleCaseJudger(JudgeContext judgeContext, JudgeContext spjContext) : base(judgeContext)
        {
            SpjJudgeContext = spjContext;
            SpjTask = spjContext.Task;
            SpjLangConfig = spjContext.LangConfig as ProgramLangConfig;
            SpjLangConfig = spjContext.LangConfig as ProgramLangConfig;
        }

        private JudgeContext SpjJudgeContext { get; }
        private JudgeTask SpjTask { get; }
        private ProgramLangConfig SpjLangConfig { get; }

        protected override CompareResult CompareAnswer(string input, string output, string userOutput)
        {
            string inputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_INPUT_NAME);
            string outputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_OUTPUT_NAME);
            string userOutputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_USER_OUTPUT_NAME);
            File.WriteAllText(inputPath, input);
            File.WriteAllText(outputPath, output);
            File.WriteAllText(userOutputPath, userOutput);

            int exitCode;
            RuntimeMonitor monitor;
            using (ProcessRunner runner = CreateSpecialJudgeProcessRunner())
            {
                runner.ProcessorAffinity = SpjTask.ProcessorAffinity;

                // 创建监视器
                monitor = new RuntimeMonitor(runner.Process, ConfigManager.Config.MonitorInterval)
                {
                    TimeLimit = SpjTask.TimeLimit,
                    TotalTimeLimit = Math.Max(SpjTask.TimeLimit * TOTAL_TIME_LIMIT_RATIO, MIN_TOTAL_TIME_LIMIT),
                    MemoryLimit = SpjTask.MemoryLimit
                };
                monitor.Start();

                runner.OutputLimit = SpjLangConfig.OutputLimit;
                exitCode = runner.Run("", out string _, out string _, ProcessPriorityClass.RealTime);
                monitor.Dispose();
            }


            File.Delete(inputPath);
            File.Delete(outputPath);
            File.Delete(userOutputPath);

            // ReSharper disable once InvertIf
            if (monitor.LimitExceed)
            {
                if (monitor.MemoryCost == monitor.MemoryLimit)
                    throw new JudgeException("Special judge program memory limit exceed");

                throw new JudgeException("Special judge program time limit exceed");
            }

            return exitCode switch
            {
                0 => CompareResult.Accepted,
                2 => CompareResult.PresentationError,
                _ => CompareResult.WrongAnswer
            };
        }

        private ProcessRunner CreateSpecialJudgeProcessRunner()
        {
            string inputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_INPUT_NAME);
            string outputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_OUTPUT_NAME);
            string userOutputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_USER_OUTPUT_NAME);

            string dataArgs = $" \"{inputPath}\" \"{outputPath}\" \"{userOutputPath}\"";

            return new ProcessRunner(
                SpjLangConfig.RunnerPath,
                SpjLangConfig.RunnerWorkDirectory,
                SpjLangConfig.RunnerArgs + dataArgs);
        }
    }
}