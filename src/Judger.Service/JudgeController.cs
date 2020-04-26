using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Judger.Adapter;
using Judger.Core;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Judge;

namespace Judger.Service
{
    /// <summary>
    /// 评测控制器
    /// </summary>
    public class JudgeController
    {
        private readonly ConcurrentQueue<JudgeContext> _judgeQueue = new ConcurrentQueue<JudgeContext>();
        private readonly object _queueLock = new object();
        private Configuration Config { get; } = ConfigManager.Config;

        public int RunningCount { get; private set; }

        public int InQueueCount => _judgeQueue.Count;

        /// <summary>
        /// 添加JudgeTask
        /// </summary>
        /// <param name="context">JudgeContext</param>
        public void AddTask(JudgeContext context)
        {
            LogAddTask(context);
            _judgeQueue.Enqueue(context);
            CheckTask();
        }

        /// <summary>
        /// 检查是否可以进行新任务
        /// </summary>
        private void CheckTask()
        {
            lock (_queueLock)
            {
                // 同时运行数达到限制或等待队列为空
                if (RunningCount >= Config.MaxRunning || _judgeQueue.IsEmpty)
                {
                    CheckExecuteGc();
                    return;
                }

                RunningCount++;
            }

            //开始评测任务
            if (_judgeQueue.TryDequeue(out JudgeContext context))
                new Task(RunJudgeTask, context).Start();
        }

        /// <summary>
        /// 开始评测JudgeTask
        /// </summary>
        /// <param name="judgeTaskObject">JudgeTask对象</param>
        private void RunJudgeTask(object judgeTaskObject)
        {
            JudgeContext context = judgeTaskObject as JudgeContext;

            try
            {
                Judge(context);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            lock (_queueLock)
                RunningCount--;

            //重新检查是否有任务
            CheckTask();
        }

        private void Judge(JudgeContext context)
        {
            JudgeTask task = context.Task;

            ITaskSubmitter submitter = AdapterFactory.CreateTaskSubmitter();

            try
            {
                UpdateTestData(context);

                using (BaseJudger judger = JudgerFactory.Create(context))
                {
                    LogStartJudgeTask(task.SubmitId);
                    judger.Judge();
                }

                LogJudgeResult(context.Result);
            }
            catch (Exception ex) //判题失败
            {
                LogJudgeFailed(ex, task.SubmitId);
                context.Result = CreateFailedJudgeResult(context, ex.ToString());
                throw;
            }
            finally
            {
                submitter.Submit(context);
            }
        }

        private void UpdateTestData(JudgeContext context)
        {
            JudgeTask task = context.Task;

            // 检查测试数据是否为最新
            if (!TestDataManager.CheckData(task.ProblemId, task.DataVersion))
            {
                LogInvalidTestData(task.ProblemId);

                ITestDataFetcher fetcher = AdapterFactory.CreateTestDataFetcher();
                TestDataManager.WriteTestData(task.ProblemId, fetcher.Fetch(context));

                LogTestDataFetched(task.ProblemId);
            }
        }

        /// <summary>
        /// 创建评测失败的JudgeResult
        /// </summary>
        /// <param name="context">JudgeContext</param>
        /// <param name="message">错误信息</param>
        /// <returns>结果为失败的JudgeResult</returns>
        private JudgeResult CreateFailedJudgeResult(JudgeContext context, string message = "")
        {
            JudgeResult result = new JudgeResult
            {
                Author = context.Task.Author,
                JudgeDetail = message,
                MemoryCost = 0,
                TimeCost = 0,
                PassRate = 0,
                ProblemId = context.Task.ProblemId,
                SubmitId = context.Task.SubmitId,
                ResultCode = JudgeResultCode.JudgeFailed
            };

            return result;
        }

        /// <summary>
        /// 若当前无运行中的任务则执行垃圾回收
        /// </summary>
        private void CheckExecuteGc()
        {
            if (RunningCount == 0)
            {
                LogGc();
                GC.Collect();
            }
        }

        private void LogAddTask(JudgeContext context)
        {
            JudgeTask task = context.Task;
            LogManager.Info(
                $"New task: SubmitID:{task.SubmitId} Language:{task.Language} " +
                $"CodeLength:{task.SourceCode.Length} ProblemID:{task.ProblemId} Author:{task.Author}");
        }

        private void LogJudgeResult(JudgeResult result)
        {
            LogManager.Info(
                $"Task {result.SubmitId} result: Time:{result.TimeCost} Mem:{result.MemoryCost} " +
                $"Code:{result.ResultCode} PassRate:{result.PassRate} Details:{result.JudgeDetail} ");
        }

        private void LogInvalidTestData(string problemId)
        {
            LogManager.Info("Invalid test data, fetch new data. ProblemID: " + problemId);
        }

        private void LogTestDataFetched(string problemId)
        {
            LogManager.Info("Problem " + problemId + " data fetched");
        }

        private void LogStartJudgeTask(string submitId)
        {
            LogManager.Info("Judge task " + submitId);
        }

        private void LogJudgeFailed(Exception ex, string submitId)
        {
            LogManager.Info("Judge failed, SubmitID: " + submitId);
            LogManager.Exception(ex);
        }

        private void LogGc()
        {
            LogManager.Info("Run Gc");
        }

        private void LogException(Exception ex)
        {
            LogManager.Exception(ex);
        }
    }
}