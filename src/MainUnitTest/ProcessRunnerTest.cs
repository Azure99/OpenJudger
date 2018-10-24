using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Judger.Utils;

namespace MainUnitTest
{
    public class ProcessRunnerTest
    {
        [Fact]
        public void TestProcessRunnerKill()
        {
            using (ProcessRunner pr = new ProcessRunner("cmd", "", ""))
            {
                new System.Threading.Thread(() => { System.Threading.Thread.Sleep(1000); pr.Process.Kill(); }).Start();
                int code = pr.Run("ping 127.0.0.1\nping 127.0.0.1", out string output, out string error);

                Assert.True(code == -1);
            }
        }

        [Fact]
        public void TestProcessRunnerRun()
        {
            using (ProcessRunner pr = new ProcessRunner("cmd", "", ""))
            {
                //new System.Threading.Thread(() => { System.Threading.Thread.Sleep(1000); pr.Process.Kill(); }).Start();
                int code = pr.Run("ping 127.0.0.1\nping 127.0.0.1", out string output, out string error);

                Assert.True(code == 0 && output.IndexOf("127.0.0.1") != -1);
            }
        }
    }
}
