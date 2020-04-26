using System.Diagnostics;
using System.Runtime.InteropServices;
using Judger.Utils;
using Xunit;

namespace MainUnitTest
{
    public class RuntimeMonitorTest
    {
        /// <summary>
        /// 测试ProcessMonitor的运行时间限制
        /// </summary>
        [Fact]
        public void TestMonitor()
        {
            string fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "bash";

            Process process = new Process();
            process.StartInfo.FileName = fileName;

            RuntimeMonitor monitor = new RuntimeMonitor(process)
            {
                MemoryLimit = 1024 * 128,
                TimeLimit = 1000,
                TotalTimeLimit = 2000
            };

            monitor.Start();
            process.Start();
            process.WaitForExit();

            Assert.True(process.ExitCode != 0);
            Assert.True(monitor.TimeCost == monitor.TimeLimit);
            Assert.True(monitor.LimitExceed);
        }
    }
}