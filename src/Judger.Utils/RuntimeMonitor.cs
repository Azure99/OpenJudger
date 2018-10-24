using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;

namespace Judger.Utils
{
    /// <summary>
    /// 运行时监控器, 可监控程序运行时间及峰值内存消耗, 可做出限制
    /// </summary>
    public class RuntimeMonitor : IDisposable
    {
        /// <summary>
        /// (CPU)时间消耗
        /// </summary>
        public int TimeCost { get; private set; } = 0;

        /// <summary>
        /// 总时间消耗
        /// </summary>
        public int TotalTimeCost { get; private set; } = 0;

        /// <summary>
        /// 内存消耗
        /// </summary>
        public int MemoryCost { get; private set; } = 0;

        /// <summary>
        /// (CPU)时间限制
        /// </summary>
        public int TimeLimit { get; set; } = 0;

        /// <summary>
        /// 最大时间限制
        /// </summary>
        public int TotalTimeLimit { get; set; } = 0;

        /// <summary>
        /// 内存限制
        /// </summary>
        public int MemoryLimit { get; set; } = 0;

        /// <summary>
        /// 是否因超出限制而Kill掉进程
        /// </summary>
        public bool LimitExceed { get; private set; } = false;

        /// <summary>
        /// 当前监控的Process
        /// </summary>
        public Process Process { get; }

        // 指示当前平台是否为Windows
        private bool _platformIsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private Timer _timer = new Timer();

        /// <summary>
        /// 运行时监控器
        /// </summary>
        /// <param name="process">需要监控的Process</param>
        /// <param name="interval">监控周期</param>
        public RuntimeMonitor(Process process, int interval = 20)
        {
            Process = process;
            _timer.Interval = interval;
            _timer.Elapsed += OnMonitor;
        }

        /// <summary>
        /// 开始监控
        /// </summary>
        public void Start()
        {
            _timer.Start();
        }

        /// <summary>
        /// 停止监控
        /// </summary>
        public void Stop()
        {
            try
            {
                CheckTimeCost();
                CheckMemoryCost();
            }
            catch { }
            _timer.Stop();
        }

        public void Dispose()
        {
            Stop();
            _timer.Dispose();
        }

        private void OnMonitor(object sender, ElapsedEventArgs e)
        {
            try//防止无效操作异常
            {
                if(Process.HasExited)
                {
                    Stop();
                    return;
                }

                if (!CheckTimeCost() || !CheckMemoryCost()) 
                {
                    Process.Kill();
                }
            }
            catch { }
        }

        private bool CheckTimeCost()
        {
            // 使用的CPU时间
            TimeCost = (int)Process.TotalProcessorTime.TotalMilliseconds;
            // 使用的总时间
            TotalTimeCost = (int)(DateTime.Now - Process.StartTime).TotalMilliseconds;

            if ((TimeCost > TimeLimit && TimeLimit > 0) ||
                (TotalTimeCost > TotalTimeLimit && TotalTimeLimit > 0))
            {
                TimeCost = TimeLimit;
                TotalTimeCost = TotalTimeLimit;
                LimitExceed = true;
                return false;
            }

            return true;
        }
        
        private bool CheckMemoryCost()
        {
            int nowMemoryCost;
            if (_platformIsWindows)//针对不同平台判断
            {
                nowMemoryCost = (int)(Process.PeakPagedMemorySize64 / 1024);
            }
            else
            {
                nowMemoryCost = (int)(Process.WorkingSet64 / 1024);
            }

            if (nowMemoryCost > MemoryCost)
            {
                MemoryCost = nowMemoryCost;
                if (MemoryCost > MemoryLimit && MemoryLimit > 0)
                {
                    MemoryCost = MemoryLimit;
                    LimitExceed = true;
                    return false;
                }
            }

            return true;
        }
    }
}
