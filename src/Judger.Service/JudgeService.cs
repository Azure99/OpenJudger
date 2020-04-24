using System;
using System.IO;
using System.Timers;
using Judger.Adapter;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Program;

namespace Judger.Service
{
    /// <summary>
    /// 评测服务
    /// </summary>
    public class JudgeService : IDisposable
    {
        // 指示当前OnWork代码段是否正在执行
        private bool _innerWorking;
        private readonly object _innerWorkLock = new object();
        private readonly ITaskFetcher _taskFetcher;
        private readonly Timer _workTimer;

        /// <summary>
        /// 评测服务
        /// </summary>
        public JudgeService()
        {
            _taskFetcher = AdapterFactory.CreateTaskFetcher();
            _workTimer = new Timer(Config.TaskFetchInterval);
            _workTimer.Elapsed += OnWork;
        }

        private Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 服务是否在运行
        /// </summary>
        public bool Working => _workTimer.Enabled;

        /// <summary>
        /// 并发判题管理器
        /// </summary>
        public JudgeController Controller { get; } = new JudgeController();

        public void Dispose()
        {
            _taskFetcher.Dispose();
            _workTimer.Dispose();
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            LogManager.Info("Start service");

            ClearTempDirectory();
            _workTimer.Start();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            LogManager.Info("Stop service");

            _workTimer.Stop();
        }

        private void ClearTempDirectory() //清空临时目录
        {
            LogManager.Info("Clear temp directory");

            foreach (ProgramLangConfig lang in Config.Languages)
            {
                try
                {
                    if (Directory.Exists(lang.JudgeDirectory))
                        Directory.Delete(lang.JudgeDirectory, true);
                }
                catch (Exception ex)
                {
                    LogManager.Error("Can not clear temp directory!");
                    LogManager.Exception(ex);
                }
            }
        }

        /// <summary>
        /// 工作事件(尝试取回JudgeTask)
        /// </summary>
        private void OnWork(object sender, ElapsedEventArgs e)
        {
            // 防止事件重入
            if (_innerWorking)
                return;

            lock (_innerWorkLock)
            {
                _innerWorking = true;

                try
                {
                    FetchJudgeTask();
                }
                catch (Exception ex)
                {
                    LogManager.Error("Can not fetch judge task!");
                    LogManager.Exception(ex, false);
                }

                _innerWorking = false;
            }
        }

        /// <summary>
        /// 从服务器取回评测任务
        /// </summary>
        private void FetchJudgeTask()
        {
            if (Controller.InQueueCount >= Config.MaxQueueSize)
                return;

            JudgeContext[] tasks = _taskFetcher.Fetch();
            foreach (JudgeContext task in tasks)
                Controller.AddTask(task);

            // 若当前成功取到任务, 不等待继续尝试取回任务
            if (tasks.Length > 0)
                FetchJudgeTask();
        }
    }
}