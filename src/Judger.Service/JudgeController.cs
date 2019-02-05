using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Judger.Entity;
using Judger.Fetcher;
using Judger.Core;
using Judger.Managers;

namespace Judger.Service
{
    /// <summary>
    /// 并发评测控制器
    /// </summary>
    public class JudgeController
    {
        /// <summary>
        /// 正在运行的评测任务数量
        /// </summary>
        public int RunningCount { get; private set; }

        /// <summary>
        /// 在等待队列中的评测任务数量
        /// </summary>
        public int InQueueCount
        {
            get
            {
                return _judgeQueue.Count;
            }
        }

        private readonly Configuration _config = ConfigManager.Config;

        /// <summary>
        /// 评测任务等待队列
        /// </summary>
        private ConcurrentQueue<JudgeTask> _judgeQueue = new ConcurrentQueue<JudgeTask>();

        private object _queueLock = new object();

        /// <summary>
        /// 添加JudgeTask
        /// </summary>
        /// <param name="task">JudgeTask</param>
        public void AddTask(JudgeTask task)
        {
            LogManager.Info(
                string.Format("New task: SubmitID:{0} Language:{1} CodeLength:{2} ProblemID:{3} Author:{4}",
                              task.SubmitID, task.Language, task.SourceCode.Length, task.ProblemID, task.Author));

            _judgeQueue.Enqueue(task);
            CheckTask();
        }

        /// <summary>
        /// 检查是否可以进行新任务
        /// </summary>
        public void CheckTask()
        {

            lock(_queueLock)
            {
                // 若同时运行数达到限制或等待队列为空
                if (RunningCount >= _config.MaxRunning || _judgeQueue.IsEmpty)
                {
                    CheckExecuteGC();
                    return;
                }

                RunningCount++;
            }


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
                LogManager.Exception(ex);
            }

            lock (_queueLock)
            {
                RunningCount--;
            }

            CheckTask();//重新检查是否有任务
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
                    LogManager.Info("Judge task " + task.SubmitID);
                    result = judger.Judge();
                }

                LogManager.Info(string.Format("Task {0} result: Time:{1} Mem:{2} Code:{3} PassRate:{4} Details:{5} ",
                                              result.SubmitID, result.TimeCost, result.MemoryCost, result.ResultCode,
                                              result.PassRate, result.JudgeDetail));
            }
            catch(Exception ex)//判题失败
            {
                LogManager.Info("Judge failed, SubmitID: " + task.SubmitID);
                LogManager.Exception(ex);
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
            if (!TestDataManager.CheckData(task.ProblemID, task.DataVersion))
            {
                LogManager.Info("Invalid test data, fetch new data. ProblemID: " + task.ProblemID);
                ITestDataFetcher fetcher = FetcherFactory.CreateTestDataFetcher();
                TestDataManager.WriteTestData(task.ProblemID, fetcher.Fetch(task));
                LogManager.Info("Problem " + task.ProblemID + " data fetched");
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
                ProblemID = task.ProblemID,
                SubmitID = task.SubmitID,
                ResultCode = JudgeResultCode.JudgeFailed
            };

            return result;
        }

        /// <summary>
        /// 若当前无运行中的任务则执行垃圾回收
        /// </summary>
        private void CheckExecuteGC()
        {
            if (RunningCount == 0)
            {
                LogManager.Info("Run GC");
                GC.Collect();
            }
        }
    }
}
