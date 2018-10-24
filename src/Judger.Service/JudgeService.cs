﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Judger.Fetcher;
using Judger.Managers;
using Judger.Models;

namespace Judger.Service
{
    /// <summary>
    /// 评测服务
    /// </summary>
    public class JudgeService : IDisposable
    {
        /// <summary>
        /// 服务是否在运行
        /// </summary>
        public bool Working
        {
            get
            {
                return _workTimer.Enabled;
            }
        }

        /// <summary>
        /// 并发判题管理器
        /// </summary>
        public JudgeManager JudgeManager { get; } = new JudgeManager();

        private ITaskFetcher _taskFetcher;
        private Timer _workTimer;
        private readonly Configuration _config = ConfigManager.Config;

        // 指示当前OnWork代码段是否正在执行
        private bool _innerWorking = false;
        private object _innerWorkLock = new object();

        /// <summary>
        /// 评测服务
        /// </summary>
        public JudgeService()
        {
            _taskFetcher = FetcherFactory.CreateTaskFetcher();
            _workTimer = new Timer(_config.TaskFetchInterval);
            _workTimer.Elapsed += OnWork;
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        public void Start()
        {
            ClearTempDirectory();
            _workTimer.Start();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            _workTimer.Stop();
        }

        public void Dispose()
        {
            _taskFetcher.Dispose();
            _workTimer.Dispose();
        }

        private bool ClearTempDirectory()//清空临时目录
        {
            foreach (var lang in _config.Languages)
            {
                try
                {
                    System.IO.Directory.Delete(lang.JudgeDirectory, true);
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 工作事件(尝试取回JudgeTask)
        /// </summary>
        private void OnWork(object sender, ElapsedEventArgs e)
        {
            // 防止事件重入
            if (_innerWorking)
            {
                return;
            }

            lock(_innerWorkLock)
            {
                _innerWorking = true;

                try
                {
                    FetchJudgeTask();
                }
                catch { }

                _innerWorking = false;
            }
        }

        /// <summary>
        /// 从服务器取回评测任务
        /// </summary>
        private void FetchJudgeTask()
        {
            if (JudgeManager.InQueueCount >= _config.MaxQueueSize)
            {
                return;
            }

            JudgeTask[] tasks = _taskFetcher.Fetch();
            foreach(var task in tasks)
            {
                JudgeManager.AddTask(task);
            }

            // 若当前成功取到任务, 不等待继续尝试取回任务
            if (tasks.Length > 0) 
            {
                FetchJudgeTask();
            }
        }
    }
}
