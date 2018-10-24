using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;
using Judger.Judger.Models;

namespace Judger.Judger
{
    /// <summary>
    /// 单例Judger
    /// </summary>
    public class SingleJudger : ISingleJudger
    {
        /// <summary>
        /// 最大总时间为CPU总时间的倍数
        /// </summary>
        private const int TOTAL_TIME_LIMIT_TUPLING = 3;

        public string RunnerPath { get; set; }
        public string RunnerWorkDirectory { get; set; } = "";
        public string RunnerArgs { get; set; } = "";
        public int TimeLimit { get; set; } = 1000;
        public int MemoryLimit { get; set; } = 262144;
        public int OutputLimit { get; set; } = 67108864;
        public IntPtr ProcessorAffinity { get; set; } = ProcessorAffinityManager.DefaultAffinity;

        public SingleJudger(string runnerPath)
        {
            RunnerPath = runnerPath;
        }

        public SingleJudgeResult Judge(string input, string output)
        {
            string userOutput = "";
            string userError = "";
            int exitcode = 0;
            RuntimeMonitor monitor;

            using (ProcessRunner runner = new ProcessRunner(RunnerPath, RunnerWorkDirectory, RunnerArgs))
            {
                runner.ProcessorAffinity = ProcessorAffinity;
                monitor = new RuntimeMonitor(runner.Process)//创建监视器
                {
                    TimeLimit = TimeLimit,
                    TotalTimeLimit = TimeLimit * TOTAL_TIME_LIMIT_TUPLING,
                    MemoryLimit = MemoryLimit
                };
                monitor.Start();

                runner.OutputLimit = OutputLimit;
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

            if ((userOutput.Length >= OutputLimit || userError.Length >= OutputLimit))
            {
                result.ResultCode = JudgeResultCode.OutputLimitExceed;
                return result;
            }

            if (monitor.LimitExceed)
            {
                result.ResultCode = (TimeLimit == monitor.TimeCost) ? JudgeResultCode.TimeLimitExceed : JudgeResultCode.MemoryLimitExceed;
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
        /// 对比结果
        /// </summary>
        public enum CompareResult
        {
            Accepted = 1,
            WrongAnswer = 2,
            PresentationError = 3
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

            string[] crtArr = correctAnswer.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');//正确答案
            string[] usrArr = userAnswer.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');//用户答案
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
    }
}
