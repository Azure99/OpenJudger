using System;
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
            using (var pr = new ProcessRunner("cmd"))
            {
                new Thread(() =>
                {
                    Thread.Sleep(1000);
                    pr.Process.Kill();
                }).Start();
                int exitCode = pr.Run("ping 127.0.0.1\nping 127.0.0.1", out string output, out string error);

                Assert.True(exitCode == -1);
            }
        }

        /// <summary>
        /// 测试ProcessRunner运行
        /// </summary>
        [Fact]
        public void TestProcessRunnerRun()
        {
            using (var pr = new ProcessRunner("cmd"))
            {
                const string cmd = "ping 127.0.0.1\nping 127.0.0.1";
                int exitCode = pr.Run(cmd, out string output, out string error);

                Assert.True(exitCode == 0);
                Assert.True(output.IndexOf("127.0.0.1", StringComparison.Ordinal) != -1);
            }
        }
    }
}