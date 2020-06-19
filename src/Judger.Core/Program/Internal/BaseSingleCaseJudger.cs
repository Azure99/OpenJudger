using System;
using System.Diagnostics;
using System.Text;
using Judger.Core.Program.Internal.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Judge;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Core.Program.Internal
{
    /// <summary>
    /// 单组用例评测基类
    /// </summary>
    public abstract class BaseSingleCaseJudger
    {
        /// <summary>
        /// 最大总时间为CPU总时间的倍数
        /// </summary>
        protected const int ConstTotalTimeLimitRatio = 5;

        /// <summary>
        /// 总时间限制x倍数的最小值
        /// </summary>
        protected const int ConstMinTotalTimeLimit = 20000;

        protected BaseSingleCaseJudger(JudgeContext context)
        {
            JudgeTask = context.Task;
            LangConfig = context.LangConfig as ProgramLangConfig;
        }

        private JudgeTask JudgeTask { get; }
        private ProgramLangConfig LangConfig { get; }

        public SingleJudgeResult Judge(string input, string output)
        {
            string userOutput;
            string userError;
            int exitCode;
            RuntimeMonitor monitor;

            using (ProcessRunner runner = CreateProcessRunner())
            {
                runner.ProcessorAffinity = JudgeTask.ProcessorAffinity;
                if (LangConfig.UseUtf8)
                    runner.Encoding = Encoding.UTF8;

                // 创建监视器
                int totalTimeLimit = Math.Max(JudgeTask.TimeLimit * ConstTotalTimeLimitRatio, ConstMinTotalTimeLimit);
                monitor = new RuntimeMonitor(runner.Process, ConfigManager.Config.MonitorInterval,
                    LangConfig.RunningInVm)
                {
                    TimeLimit = JudgeTask.TimeLimit,
                    TotalTimeLimit = totalTimeLimit,
                    MemoryLimit = JudgeTask.MemoryLimit
                };
                monitor.Start();

                runner.OutputLimit = LangConfig.OutputLimit;
                exitCode = runner.Run(input, out userOutput, out userError,
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

            if (userOutput.Length >= LangConfig.OutputLimit ||
                userError.Length >= LangConfig.OutputLimit)
            {
                result.ResultCode = JudgeResultCode.OutputLimitExceed;
                return result;
            }

            if (monitor.LimitExceed)
            {
                result.ResultCode = JudgeTask.TimeLimit == monitor.TimeCost
                    ? JudgeResultCode.TimeLimitExceed
                    : JudgeResultCode.MemoryLimitExceed;
                return result;
            }

            if (exitCode != 0)
            {
                result.ResultCode = JudgeResultCode.RuntimeError;
                return result;
            }

            CompareResult cmpResult = CompareAnswer(input, output, userOutput); // 对比答案输出
            if (cmpResult == CompareResult.Accepted)
                result.ResultCode = JudgeResultCode.Accepted;
            else if (cmpResult == CompareResult.PresentationError)
                result.ResultCode = JudgeResultCode.PresentationError;
            else
                result.ResultCode = JudgeResultCode.WrongAnswer;

            return result;
        }

        private ProcessRunner CreateProcessRunner()
        {
            return new ProcessRunner(
                LangConfig.RunnerPath,
                LangConfig.RunnerWorkDirectory,
                LangConfig.RunnerArgs);
        }

        protected abstract CompareResult CompareAnswer(string input, string correctAnswer, string userAnswer);
    }
}