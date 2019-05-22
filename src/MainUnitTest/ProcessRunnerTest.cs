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
            using (ProcessRunner pr = new ProcessRunner("cmd"))
            {
                new Thread(() =>
                {
                    Thread.Sleep(1000);
                    pr.Process.Kill();
                }).Start();
                int exitcode = pr.Run("ping 127.0.0.1\nping 127.0.0.1", out string output, out string error);

                Assert.True(exitcode == -1);
            }
        }

        /// <summary>
        /// 测试ProcessRunner运行
        /// </summary>
        [Fact]
        public void TestProcessRunnerRun()
        {
            using (ProcessRunner pr = new ProcessRunner("cmd"))
            {
                string cmd = "ping 127.0.0.1\nping 127.0.0.1";
                int exitcode = pr.Run(cmd, out string output, out string error);

                Assert.True(exitcode == 0 && output.IndexOf("127.0.0.1") != -1);
            }
        }
    }
}