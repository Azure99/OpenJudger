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
        private readonly Configuration _config = ConfigManager.Config;
        private readonly object _innerWorkLock = new object();
        private readonly ITaskFetcher _taskFetcher;

        private readonly Timer _workTimer;

        // 指示当前OnWork代码段是否正在执行
        private bool _innerWorking;

        public JudgeService()
        {
            _taskFetcher = AdapterFactory.CreateTaskFetcher();
            _workTimer = new Timer(_config.TaskFetchInterval);
            _workTimer.Elapsed += OnWork;
        }

        /// <summary>
        /// 服务是否在运行
        /// </summary>
        public bool Working => _workTimer.Enabled;

        /// <summary>
        /// 评测控制器
        /// </summary>
        public JudgeController Controller { get; } = new JudgeController();

        public void Dispose()
        {
            _taskFetcher.Dispose();
            _workTimer.Dispose();
        }

        public void Start()
        {
            LogManager.Info("Start service");
            ClearTempDirectory();
            _workTimer.Start();
        }

        // ReSharper disable once UnusedMember.Global
        public void Stop()
        {
            LogManager.Info("Stop service");
            _workTimer.Stop();
        }

        private void ClearTempDirectory()
        {
            LogManager.Info("Clear temp directory");

            foreach (ProgramLangConfig lang in _config.Languages)
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

        private void FetchJudgeTask()
        {
            while (true)
            {
                if (Controller.PendingCount >= _config.MaxQueueSize)
                    break;

                JudgeContext[] tasks = _taskFetcher.Fetch();
                if (tasks.Length <= 0)
                    break;

                foreach (JudgeContext task in tasks)
                    Controller.AddTask(task);
            }
        }
    }
}