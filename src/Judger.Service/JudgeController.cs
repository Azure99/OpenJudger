using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Judger.Core;
using Judger.Fetcher;
using Judger.Managers;
using Judger.Models;

namespace Judger.Service
{
    /// <summary>
    /// 并发评测控制器
    /// </summary>
    public class JudgeController
    {
        // 评测任务等待队列
        private ConcurrentQueue<JudgeContext> _judgeQueue = new ConcurrentQueue<JudgeContext>();
        private object _queueLock = new object();
        private Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 正在运行的评测任务数量
        /// </summary>
        public int RunningCount { get; private set; }

        /// <summary>
        /// 在等待队列中的评测任务数量
        /// </summary>
        public int InQueueCount
        {
            get { return _judgeQueue.Count; }
        }

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
        public void CheckTask()
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
            {
                RunningCount--;
            }

            //重新检查是否有任务
            CheckTask();
        }

        /// <summary>
        /// 评测此JudgeTask
        /// </summary>
        private void Judge(JudgeContext context)
        {
            JudgeTask task = context.Task;
            
            ITaskSubmitter submitter = FetcherFactory.CreateTaskSubmitter();

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
                throw ex;
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

                ITestDataFetcher fetcher = FetcherFactory.CreateTestDataFetcher();
                TestDataManager.WriteTestData(task.ProblemId, fetcher.Fetch(context));

                LogTestDataFetched(task.ProblemId);
            }
        }

        /// <summary>
        /// 创建评测失败的JudgeResult
        /// </summary>
        /// <param name="task">JudgeTask</param>
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
            LogManager.Info(string.Format(
                "New task: SubmitID:{0} Language:{1} CodeLength:{2} ProblemID:{3} Author:{4}",
                task.SubmitId, task.Language, task.SourceCode.Length, task.ProblemId, task.Author));
        }

        private void LogJudgeResult(JudgeResult result)
        {
            LogManager.Info(string.Format(
                "Task {0} result: Time:{1} Mem:{2} Code:{3} PassRate:{4} Details:{5} ",
                result.SubmitId, result.TimeCost, result.MemoryCost, result.ResultCode,
                result.PassRate, result.JudgeDetail));
        }

        private void LogInvalidTestData(int problemId)
        {
            LogManager.Info("Invalid test data, fetch new data. ProblemID: " + problemId);
        }

        private void LogTestDataFetched(int problemId)
        {
            LogManager.Info("Problem " + problemId + " data fetched");
        }

        private void LogStartJudgeTask(int submitId)
        {
            LogManager.Info("Judge task " + submitId);
        }

        private void LogJudgeFailed(Exception ex, int submitId)
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