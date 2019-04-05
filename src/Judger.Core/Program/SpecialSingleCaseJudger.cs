using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Judger.Core.Program.Entity;
using Judger.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Core.Program
{
    public class SpecialSingleCaseJudger
    {
        public JudgeTask JudgeTask { get; }
        public ProgramLangConfig LangConfig { get { return JudgeTask.LangConfig as ProgramLangConfig; } }
        public JudgeTask SPJTask { get; }
        public ProgramLangConfig SPJLangConfig { get { return SPJTask.LangConfig as ProgramLangConfig; } }

        private const string SPJ_INPUT_NAME = "input.txt";
        private const string SPJ_OUTPUT_NAME = "output.txt";
        private const string SPJ_USEROUTPUT_NAME = "user.txt";

        /// <summary>
        /// 最大总时间为CPU总时间的倍数
        /// </summary>
        private const int TOTAL_TIME_LIMIT_TUPLING = 3;

        private ProgramLangConfig _langConfig;
        public SpecialSingleCaseJudger(JudgeTask judgeTask, JudgeTask spjTask)
        {
            JudgeTask = judgeTask;
            _langConfig = judgeTask.LangConfig as ProgramLangConfig;
            SPJTask = spjTask;
        }

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
                {
                    runner.Encoding = Encoding.UTF8;
                }

                // 创建监视器
                monitor = new RuntimeMonitor(runner.Process, ConfigManager.Config.MonitorInterval)
                {
                    TimeLimit = JudgeTask.TimeLimit,
                    TotalTimeLimit = JudgeTask.TimeLimit * TOTAL_TIME_LIMIT_TUPLING,
                    MemoryLimit = JudgeTask.MemoryLimit
                };
                monitor.Start();

                runner.OutputLimit = _langConfig.OutputLimit;
                exitcode = runner.Run(input, out userOutput, out userError, ProcessPriorityClass.RealTime);
                monitor.Dispose();
            }

            SingleJudgeResult result = new SingleJudgeResult
            {
                TimeCost = monitor.TimeCost,
                MemoryCost = monitor.MemoryCost,
                JudgeDetail = userError,
                ResultCode = JudgeResultCode.Accepted
            };

            if ((userOutput.Length >= _langConfig.OutputLimit ||
                userError.Length >= _langConfig.OutputLimit))
            {
                result.ResultCode = JudgeResultCode.OutputLimitExceed;
                return result;
            }

            if (monitor.LimitExceed)
            {
                result.ResultCode = (JudgeTask.TimeLimit == monitor.TimeCost) ?
                                     JudgeResultCode.TimeLimitExceed :
                                     JudgeResultCode.MemoryLimitExceed;
                return result;
            }

            if (exitcode != 0)//判断是否运行错误
            {
                result.ResultCode = JudgeResultCode.RuntimeError;
                return result;
            }

            CompareResult cmpResult = CompareAnswerBySPJ(input, output, userOutput);//对比答案输出
            if (cmpResult == CompareResult.Accepted)
            {
                result.ResultCode = JudgeResultCode.Accepted;
            }
            else if (cmpResult == CompareResult.PresentationError)
            {
                result.ResultCode = JudgeResultCode.PresentationError;
            }
            else
            {
                result.ResultCode = JudgeResultCode.WrongAnswer;
            }

            return result;
        }

        private CompareResult CompareAnswerBySPJ(string input, string output, string userOutput)
        {
            string inputPath = Path.Combine(SPJTask.TempJudgeDirectory, SPJ_INPUT_NAME);
            string outputPath = Path.Combine(SPJTask.TempJudgeDirectory, SPJ_OUTPUT_NAME);
            string userOutputPath = Path.Combine(SPJTask.TempJudgeDirectory, SPJ_USEROUTPUT_NAME);
            File.WriteAllText(inputPath, input);
            File.WriteAllText(outputPath, output);
            File.WriteAllText(userOutputPath, userOutput);


            int exitcode = 0;
            RuntimeMonitor monitor;
            using (ProcessRunner runner = CreateSpecialJudgeProcessRunner())
            {
                runner.ProcessorAffinity = SPJTask.ProcessorAffinity;

                // 创建监视器
                monitor = new RuntimeMonitor(runner.Process, ConfigManager.Config.MonitorInterval)
                {
                    TimeLimit = SPJTask.TimeLimit,
                    TotalTimeLimit = SPJTask.TimeLimit * TOTAL_TIME_LIMIT_TUPLING,
                    MemoryLimit = SPJTask.MemoryLimit
                };
                monitor.Start();

                runner.OutputLimit = SPJLangConfig.OutputLimit;
                exitcode = runner.Run("", out string o, out string e, ProcessPriorityClass.RealTime);
                monitor.Dispose();
            }
            

            File.Delete(inputPath);
            File.Delete(outputPath);
            File.Delete(userOutputPath);

            if (monitor.LimitExceed)
            {
                if (monitor.MemoryCost == monitor.MemoryLimit)
                {
                    throw new JudgeException("Special judge program memory limit exceed");
                }
                else
                {
                    throw new JudgeException("Special judge program time limit exceed");
                }
            }

            switch (exitcode)
            {
                case 0:  return CompareResult.Accepted;
                case 2:  return CompareResult.PresentationError;
                default: return CompareResult.WrongAnswer;
            }
        }

        private ProcessRunner CreateProcessRunner()
        {
            return new ProcessRunner(
                _langConfig.RunnerPath,
                _langConfig.RunnerWorkDirectory,
                _langConfig.RunnerArgs);
        }

        private ProcessRunner CreateSpecialJudgeProcessRunner()
        {
            string inputPath = Path.Combine(SPJTask.TempJudgeDirectory, SPJ_INPUT_NAME);
            string outputPath = Path.Combine(SPJTask.TempJudgeDirectory, SPJ_OUTPUT_NAME);
            string userOutputPath = Path.Combine(SPJTask.TempJudgeDirectory, SPJ_USEROUTPUT_NAME);

            string dataArgs = string.Format(" \"{0}\" \"{1}\" \"{2}\"", inputPath, outputPath, userOutputPath);

            return new ProcessRunner(
                SPJLangConfig.RunnerPath,
                SPJLangConfig.RunnerWorkDirectory,
                SPJLangConfig.RunnerArgs + dataArgs);
        }
    }
}
