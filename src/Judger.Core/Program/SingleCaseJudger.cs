using System;
using System.Diagnostics;
using System.Text;
using Judger.Entity;
using Judger.Core.Program.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Core.Program
{
    /// <summary>
    /// 单例Judger
    /// </summary>
    public class SingleCaseJudger
    {
        /// <summary>
        /// 最大总时间为CPU总时间的倍数
        /// </summary>
        private const int TOTAL_TIME_LIMIT_TUPLING = 5;

        /// <summary>
        /// 总时间限制x倍数的最小值
        /// </summary>
        private const int MIN_TOTAL_TIME_LIMIT = 20000;

        public JudgeTask JudgeTask { get; set; }

        public ProgramLangConfig LangConfig { get { return JudgeTask.LangConfig as ProgramLangConfig; } }

        private ProgramLangConfig _langConfig;

        public SingleCaseJudger(JudgeTask task)
        {
            JudgeTask = task;
            _langConfig = task.LangConfig as ProgramLangConfig;
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
                int totalTimeLimit = Math.Max(JudgeTask.TimeLimit * TOTAL_TIME_LIMIT_TUPLING, MIN_TOTAL_TIME_LIMIT);
                monitor = new RuntimeMonitor(runner.Process, ConfigManager.Config.MonitorInterval)
                {
                    TimeLimit = JudgeTask.TimeLimit,
                    TotalTimeLimit = totalTimeLimit,
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

            CompareResult cmpResult = CompareAnswer(output, userOutput);//对比答案输出
            if(cmpResult == CompareResult.Accepted)
            {
                result.ResultCode = JudgeResultCode.Accepted;
            }
            else if(cmpResult == CompareResult.PresentationError)
            {
                result.ResultCode = JudgeResultCode.PresentationError;
            }
            else
            {
                result.ResultCode = JudgeResultCode.WrongAnswer;
            }

            return result;
        }

        /// <summary>
        /// 对比标准答案与用户答案
        /// </summary>
        /// <param name="correctAnswer">正确答案</param>
        /// <param name="userAnswer">用户答案</param>
        /// <returns>对比结果</returns>
        public CompareResult CompareAnswer(string correctAnswer, string userAnswer)
        {
            if(correctAnswer == userAnswer)
            {
                return CompareResult.Accepted;
            }

            // 正确答案
            string[] crtArr = correctAnswer.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            // 用户答案
            string[] usrArr = userAnswer.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            int crtLength = crtArr.Length;
            int usrLength = usrArr.Length;

            //替代TrimEnd('\n'), 以减少内存开销
            while(crtLength > 0 && string.IsNullOrEmpty(crtArr[crtLength - 1]))
            {
                crtLength--;
            }

            while (usrLength > 0 && string.IsNullOrEmpty(usrArr[usrLength - 1]))
            {
                usrLength--;
            }

            if (crtLength == usrLength)
            {
                bool correct = true;
                for (int i = 0; i < crtLength; i++) 
                {
                    if(crtArr[i] != usrArr[i])
                    {
                        correct = false;
                        break;
                    }
                }

                if(correct) //Accepted
                {
                    return CompareResult.Accepted;
                }
            }

            bool wrongAnser = false;
            //判断PE不再重新生成去空数组，减少时空开销
            int crtPos = 0;
            int usrPos = 0;
            while (crtPos < crtLength && usrPos < usrLength)
            {
                bool jump = false;
                while(crtPos < crtLength && crtArr[crtPos] == "")//跳过空白行
                {
                    crtPos++;
                    jump = true;
                }

                while(usrPos < usrLength && usrArr[usrPos] == "")
                {
                    usrPos++;
                    jump = true;
                }

                if(jump)
                {
                    continue;
                }

                if(usrArr[usrPos].Trim() != crtArr[crtPos].Trim())
                {
                    wrongAnser = true;
                    break;
                }
                else
                {
                    crtPos++;
                    usrPos++;
                }
            }

            while (crtPos < crtLength && crtArr[crtPos] == "")//跳过空白行
            {
                crtPos++;
            }

            while (usrPos < usrLength && usrArr[usrPos] == "")
            {
                usrPos++;
            }

            if (crtPos != crtLength || usrPos != usrLength)
            {
                wrongAnser = true;
            }
            
            return wrongAnser ? CompareResult.WrongAnswer : CompareResult.PresentationError;
        }

        private ProcessRunner CreateProcessRunner()
        {
            return new ProcessRunner(
                _langConfig.RunnerPath,
                _langConfig.RunnerWorkDirectory,
                _langConfig.RunnerArgs);
        }
    }
}
