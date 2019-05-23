using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Judger.Core.Program.Internal.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Core.Program.Internal
{
    public class SpecialSingleCaseJudger
    {
        /// <summary>
        /// 最大总时间为CPU总时间的倍数
        /// </summary>
        private const int TOTAL_TIME_LIMIT_TUPLING = 5;

        /// <summary>
        /// 总时间限制x倍数的最小值
        /// </summary>
        private const int MIN_TOTAL_TIME_LIMIT = 20000;

        private const string SPJ_INPUT_NAME = "input.txt";
        private const string SPJ_OUTPUT_NAME = "output.txt";
        private const string SPJ_USEROUTPUT_NAME = "user.txt";

        public SpecialSingleCaseJudger(JudgeContext judgeContext, JudgeContext spjContext)
        {
            JudgeContext = judgeContext;
            SpjJudgeContext = spjContext;
            JudgeTask = judgeContext.Task;
            SpjTask = spjContext.Task;
            LangConfig = judgeContext.LangConfig as ProgramLangConfig;
            SpjLangConfig = spjContext.LangConfig as ProgramLangConfig;
        }

        public JudgeContext JudgeContext { get; }

        public JudgeTask JudgeTask { get; }

        public ProgramLangConfig LangConfig { get; }

        public JudgeContext SpjJudgeContext { get; }
        public JudgeTask SpjTask { get; }

        public ProgramLangConfig SpjLangConfig { get; }

        public SingleJudgeResult Judge(string input, string output)
        {
            string userOutput = "";
            string userError = "";
            int exitcode = 0;
            RuntimeMonitor monitor;

            using (ProcessRunner runner = CreateProcessRunner())
            {
                runner.ProcessorAffinity = JudgeTask.ProcessorAffinity;
                if (LangConfig.UseUTF8)
                    runner.Encoding = Encoding.UTF8;

                // 创建监视器
                int totalTimeLimit = Math.Max(JudgeTask.TimeLimit * TOTAL_TIME_LIMIT_TUPLING, MIN_TOTAL_TIME_LIMIT);
                monitor = new RuntimeMonitor(runner.Process, ConfigManager.Config.MonitorInterval,
                    LangConfig.RunningInVm)
                {
                    TimeLimit = JudgeTask.TimeLimit,
                    TotalTimeLimit = totalTimeLimit,
                    MemoryLimit = JudgeTask.MemoryLimit
                };
                monitor.Start();

                runner.OutputLimit = LangConfig.OutputLimit;
                exitcode = runner.Run(input, out userOutput, out userError,
                    ProcessPriorityClass.RealTime, ConfigManager.Config.MonitorInterval * 2);

                monitor.Dispose();
            }

            SingleJudgeResult result = new SingleJudgeResult
            {
                TimeCost = monitor.TimeCost,
                MemoryCost = monitor.MemoryCost,
                JudgeDetail = userError,
                ResultCode = JudgeResultCode.Accepted
            };

            if ((userOutput.Length >= LangConfig.OutputLimit ||
                 userError.Length >= LangConfig.OutputLimit))
            {
                result.ResultCode = JudgeResultCode.OutputLimitExceed;
                return result;
            }

            if (monitor.LimitExceed)
            {
                result.ResultCode = (JudgeTask.TimeLimit == monitor.TimeCost)
                    ? JudgeResultCode.TimeLimitExceed
                    : JudgeResultCode.MemoryLimitExceed;
                return result;
            }

            if (exitcode != 0) //判断是否运行错误
            {
                result.ResultCode = JudgeResultCode.RuntimeError;
                return result;
            }

            CompareResult cmpResult = CompareAnswerBySpj(input, output, userOutput); //对比答案输出
            if (cmpResult == CompareResult.Accepted)
                result.ResultCode = JudgeResultCode.Accepted;
            else if (cmpResult == CompareResult.PresentationError)
                result.ResultCode = JudgeResultCode.PresentationError;
            else
                result.ResultCode = JudgeResultCode.WrongAnswer;

            return result;
        }

        private CompareResult CompareAnswerBySpj(string input, string output, string userOutput)
        {
            string inputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_INPUT_NAME);
            string outputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_OUTPUT_NAME);
            string userOutputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_USEROUTPUT_NAME);
            File.WriteAllText(inputPath, input);
            File.WriteAllText(outputPath, output);
            File.WriteAllText(userOutputPath, userOutput);


            int exitcode = 0;
            RuntimeMonitor monitor;
            using (ProcessRunner runner = CreateSpecialJudgeProcessRunner())
            {
                runner.ProcessorAffinity = SpjTask.ProcessorAffinity;

                // 创建监视器
                monitor = new RuntimeMonitor(runner.Process, ConfigManager.Config.MonitorInterval)
                {
                    TimeLimit = SpjTask.TimeLimit,
                    TotalTimeLimit = SpjTask.TimeLimit * TOTAL_TIME_LIMIT_TUPLING,
                    MemoryLimit = SpjTask.MemoryLimit
                };
                monitor.Start();

                runner.OutputLimit = SpjLangConfig.OutputLimit;
                exitcode = runner.Run("", out string o, out string e, ProcessPriorityClass.RealTime);
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

            switch (exitcode)
            {
                case 0: return CompareResult.Accepted;
                case 2: return CompareResult.PresentationError;
                default: return CompareResult.WrongAnswer;
            }
        }

        private ProcessRunner CreateProcessRunner()
        {
            return new ProcessRunner(
                LangConfig.RunnerPath,
                LangConfig.RunnerWorkDirectory,
                LangConfig.RunnerArgs);
        }

        private ProcessRunner CreateSpecialJudgeProcessRunner()
        {
            string inputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_INPUT_NAME);
            string outputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_OUTPUT_NAME);
            string userOutputPath = Path.Combine(SpjJudgeContext.TempDirectory, SPJ_USEROUTPUT_NAME);

            string dataArgs = string.Format(" \"{0}\" \"{1}\" \"{2}\"", inputPath, outputPath, userOutputPath);

            return new ProcessRunner(
                SpjLangConfig.RunnerPath,
                SpjLangConfig.RunnerWorkDirectory,
                SpjLangConfig.RunnerArgs + dataArgs);
        }
    }
}