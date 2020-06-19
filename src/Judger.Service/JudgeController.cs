using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Judger.Adapter;
using Judger.Core;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;

namespace Judger.Service
{
    /// <summary>
    /// 评测控制器
    /// </summary>
    public class JudgeController
    {
        private readonly object _checkRunLock = new object();
        private readonly Configuration _config = ConfigManager.Config;
        private readonly ConcurrentQueue<JudgeContext> _judgeQueue = new ConcurrentQueue<JudgeContext>();

        /// <summary>
        /// 正在运行的任务数
        /// </summary>
        public int RunningCount { get; private set; }

        /// <summary>
        /// 正在等待队列中的任务数
        /// </summary>
        public int PendingCount => _judgeQueue.Count;

        /// <summary>
        /// 添加评测任务(上下文)
        /// </summary>
        /// 请使用Judger.Adapter.JudgeContextFactory创建评测上下文
        public void AddTask(JudgeContext context)
        {
            LogAddTask(context);
            _judgeQueue.Enqueue(context);
            CheckRunTask();
        }

        /// <summary>
        /// 检查是否可以进行新任务, 如果是, 进行新任务
        /// </summary>
        private void CheckRunTask()
        {
            lock (_checkRunLock)
            {
                if (RunningCount == 0 && _judgeQueue.IsEmpty)
                {
                    LogGc();
                    GC.Collect();
                }

                if (!_judgeQueue.IsEmpty && RunningCount < _config.MaxRunning)
                {
                    RunningCount++;
                    if (_judgeQueue.TryDequeue(out JudgeContext context))
                        new Task(RunJudgeTask, context).Start();
                }
            }
        }

        private void RunJudgeTask(object contextObject)
        {
            JudgeContext context = contextObject as JudgeContext;

            try
            {
                Judge(context);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            lock (_checkRunLock)
            {
                RunningCount--;
            }

            //重新检查是否有任务
            CheckRunTask();
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
            }
            catch (ExpectedJudgeStopException)
            { }
            catch (Exception ex) // 判题失败
            {
                context.Result = CreateFailedJudgeResult(context, ex.ToString());
                throw;
            }
            finally
            {
                LogJudgeResult(context.Result);
                submitter.Submit(context);
            }
        }

        private void UpdateTestData(JudgeContext context)
        {
            JudgeTask task = context.Task;

            // 检查本地测试数据是否为最新
            if (!TestDataManager.CheckDataVersion(task.ProblemId, task.DataVersion))
            {
                LogInvalidTestData(task.ProblemId);

                ITestDataFetcher fetcher = AdapterFactory.CreateTestDataFetcher();
                TestDataManager.WriteTestData(task.ProblemId, fetcher.Fetch(context));

                LogTestDataFetched(task.ProblemId);
            }
        }

        private JudgeResult CreateFailedJudgeResult(JudgeContext context, string message = "")
        {
            return new JudgeResult
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

        private void LogGc()
        {
            LogManager.Info("Run GC");
        }

        private void LogException(Exception ex)
        {
            LogManager.Exception(ex);
        }
    }
}