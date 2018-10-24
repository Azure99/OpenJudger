﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Judger.Managers;
using Judger.Fetcher;
using Judger.Judger;
using Judger.Models;

namespace Judger.Service
{
    /// <summary>
    /// 并发评测管理器
    /// </summary>
    public class JudgeManager
    {
        /// <summary>
        /// 正在运行的JudeTask数
        /// </summary>
        public int RunningCount { get; private set; }

        /// <summary>
        /// 在等待队列中的JudgeTask数
        /// </summary>
        public int InQueueCount
        {
            get
            {
                return _judgeQueue.Count;
            }
        }

        private readonly Configuration _config = ConfigManager.Config;
        private object _queueLock = new object();

        // 等待队列
        private ConcurrentQueue<JudgeTask> _judgeQueue = new ConcurrentQueue<JudgeTask>();

        public JudgeManager()
        { }

        /// <summary>
        /// 添加JudgeTask
        /// </summary>
        /// <param name="task">JudgeTask</param>
        public void AddTask(JudgeTask task)
        {
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
                Console.WriteLine(ex);
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

            try
            {
                // 检查测试数据是否为最新
                if (!TestDataManager.CheckData(task.ProblemID, task.DataVersion))
                {
                    ITestDataFetcher fetcher = FetcherFactory.CreateTestDataFetcher();
                    TestDataManager.WriteTestData(task.ProblemID, fetcher.Fetch(task.ProblemID));
                }

                JudgeResult result;
                using (MainJudger judger = new MainJudger(task))
                {
                    result = judger.Judge();
                }

                submitter.Submit(result);
            }
            catch(Exception ex)//判题失败
            {
                submitter.Submit(CreateFailedJudgeResult(task, ex.ToString()));
                throw ex;
            }
        }

        /// <summary>
        /// 创建失败的JudgeResult
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
                GC.Collect();
            }
        }
    }
}
