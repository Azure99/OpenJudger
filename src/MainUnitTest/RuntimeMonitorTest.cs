using System.Diagnostics;
using Xunit;
using Judger.Utils;

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
            Process process = new Process();
            process.StartInfo.FileName = "cmd";

            RuntimeMonitor monitor = new RuntimeMonitor(process)
            {
                MemoryLimit = 100000,
                TimeLimit = 3000,
                TotalTimeLimit = 5000
            };

            monitor.Start();
            process.Start();
            process.WaitForExit();

            Assert.True(process.ExitCode == -1);
            Assert.True(monitor.TimeCost == monitor.TimeLimit);
            Assert.True(monitor.LimitExceed);
        }
    }
}
