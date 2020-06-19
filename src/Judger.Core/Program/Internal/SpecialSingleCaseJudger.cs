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
        private const string ConstSpjInputName = "input.txt";
        private const string ConstSpjOutputName = "output.txt";
        private const string ConstSpjUserOutputName = "user.txt";

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
            string inputPath = Path.Combine(SpjJudgeContext.TempDirectory, ConstSpjInputName);
            string outputPath = Path.Combine(SpjJudgeContext.TempDirectory, ConstSpjOutputName);
            string userOutputPath = Path.Combine(SpjJudgeContext.TempDirectory, ConstSpjUserOutputName);
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
                    TotalTimeLimit = Math.Max(SpjTask.TimeLimit * ConstTotalTimeLimitRatio, ConstMinTotalTimeLimit),
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

            if (monitor.LimitExceed)
            {
                if (monitor.MemoryCost == monitor.MemoryLimit)
                    throw new JudgeException("Special judge program memory limit exceed");

                throw new JudgeException("Special judge program time limit exceed");
            }

            switch (exitCode)
            {
                case 0:
                    return CompareResult.Accepted;
                case 2:
                    return CompareResult.PresentationError;
                default:
                    return CompareResult.WrongAnswer;
            }
        }

        private ProcessRunner CreateSpecialJudgeProcessRunner()
        {
            string inputPath = Path.Combine(SpjJudgeContext.TempDirectory, ConstSpjInputName);
            string outputPath = Path.Combine(SpjJudgeContext.TempDirectory, ConstSpjOutputName);
            string userOutputPath = Path.Combine(SpjJudgeContext.TempDirectory, ConstSpjUserOutputName);

            string dataArgs = $" \"{inputPath}\" \"{outputPath}\" \"{userOutputPath}\"";

            return new ProcessRunner(
                SpjLangConfig.RunnerPath,
                SpjLangConfig.RunnerWorkDirectory,
                SpjLangConfig.RunnerArgs + dataArgs);
        }
    }
}