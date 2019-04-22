using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Judger.Core;
using Judger.Entity;
using Judger.Fetcher;
using Judger.Managers;

namespace Judger.Service
{
    /// <summary>
    /// 并发评测控制器
    /// </summary>
    public class JudgeController
    {
        private readonly Configuration _config = ConfigManager.Config;
        // 评测任务等待队列
        private ConcurrentQueue<JudgeTask> _judgeQueue = new ConcurrentQueue<JudgeTask>();
        private object _queueLock = new object();

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
        /// <param name="task">JudgeTask</param>
        public void AddTask(JudgeTask task)
        {
            LogAddTask(task);
            _judgeQueue.Enqueue(task);
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
                if (RunningCount >= _config.MaxRunning || _judgeQueue.IsEmpty)
                {
                    CheckExecuteGc();
                    return;
                }

                RunningCount++;
            }

            //开始评测任务
            if (_judgeQueue.TryDequeue(out JudgeTask task))
            {
                new Task(RunJudgeTask, task).Start();
            }
        }

        /// <summary>
        /// 开始评测JudgeTask
        /// </summary>
        /// <param name="judgeTaskObject">JudgeTask对象</param>
        private void RunJudgeTask(object judgeTaskObject)
        {
            JudgeTask task = judgeTaskObject as JudgeTask;

            try
            {
                Judge(task);
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
        private void Judge(JudgeTask task)
        {
            ITaskSubmitter submitter = FetcherFactory.CreateTaskSubmitter();
            JudgeResult result = CreateFailedJudgeResult(task);

            try
            {
                UpdateTestData(task);

                using (BaseJudger judger = JudgerFactory.Create(task))
                {
                    LogStartJudgeTask(task.SubmitId);
                    result = judger.Judge();
                }

                LogJudgeResult(result);
            }
            catch (Exception ex)//判题失败
            {
                LogJudgeFailed(ex, task.SubmitId);
                result.JudgeDetail = ex.ToString();
                throw ex;
            }
            finally
            {
                submitter.Submit(result);
            }
        }

        private void UpdateTestData(JudgeTask task)
        {
            // 检查测试数据是否为最新
            if (!TestDataManager.CheckData(task.ProblemId, task.DataVersion))
            {
                LogInvalidTestData(task.ProblemId);

                ITestDataFetcher fetcher = FetcherFactory.CreateTestDataFetcher();
                TestDataManager.WriteTestData(task.ProblemId, fetcher.Fetch(task));

                LogTestDataFetched(task.ProblemId);
            }
        }

        /// <summary>
        /// 创建评测失败的JudgeResult
        /// </summary>
        /// <param name="task">JudgeTask</param>
        /// <param name="message">错误信息</param>
        /// <returns>结果为失败的JudgeResult</returns>
        private JudgeResult CreateFailedJudgeResult(JudgeTask task, string message = "")
        {
            JudgeResult result = new JudgeResult
            {
                Author = task.Author,
                JudgeDetail = message,
                MemoryCost = 0,
                TimeCost = 0,
                PassRate = 0,
                ProblemId = task.ProblemId,
                SubmitId = task.SubmitId,
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
                LogGC();
                GC.Collect();
            }
        }

        private void LogAddTask(JudgeTask task)
        {
            LogManager.Info(string.Format("New task: SubmitID:{0} Language:{1} CodeLength:{2} ProblemID:{3} Author:{4}",
                                task.SubmitId, task.Language, task.SourceCode.Length, task.ProblemId, task.Author));
        }

        private void LogJudgeResult(JudgeResult result)
        {
            LogManager.Info(string.Format("Task {0} result: Time:{1} Mem:{2} Code:{3} PassRate:{4} Details:{5} ",
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

        private void LogGC()
        {
            LogManager.Info("Run GC");
        }

        private void LogException(Exception ex)
        {
            LogManager.Exception(ex);
        }
    }
}
