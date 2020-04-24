using System;
using System.Runtime.InteropServices;
using System.Threading;
using Judger.Utils;
using Xunit;

namespace MainUnitTest
{
    public class ProcessRunnerTest
    {
        /// <summary>
        /// 测试ProcessRunner杀死进程
        /// </summary>
        [Fact]
        public void TestProcessRunnerKill()
        {
            using (var runner = new ProcessRunner(GetFileName()))
            {
                new Thread(() =>
                {
                    Thread.Sleep(1000);
                    runner.Process.Kill();
                }).Start();
                int exitCode = runner.Run(GetCommand(), out string output, out string error);

                Assert.True(exitCode != 0);
            }
        }

        /// <summary>
        /// 测试ProcessRunner运行及I/O操作
        /// </summary>
        [Fact]
        public void TestProcessRunnerRun()
        {
            using (var runner = new ProcessRunner(GetFileName()))
            {
                int exitCode = runner.Run(GetCommand(), out string output, out string error);

                Assert.True(exitCode == 0);
                Assert.True(output.IndexOf("127.0.0.1", StringComparison.Ordinal) != -1);
            }
        }

        private String GetFileName()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "bash";
        }

        private String GetCommand()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "ping 127.0.0.1\nping 127.0.0.1"
                : "ping 127.0.0.1 -c4\nping 127.0.0.1 -c4";
        }
    }
}