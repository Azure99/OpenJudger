using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Judger.Utils
{
    /// <summary>
    /// 进程运行器
    /// 运行指定程序并重定向标准I/O流, 支持输出限制
    /// </summary>
    public class ProcessRunner : IDisposable
    {
        // 读输出/错误流时的休眠周期
        private const int ConstReadSleepInterval = 10;

        /// <summary>
        /// 程序运行器
        /// </summary>
        /// <param name="fileName">程序文件名</param>
        /// <param name="workDirectory">工作目录</param>
        /// <param name="args">运行参数</param>
        public ProcessRunner(string fileName, string workDirectory = "", string args = "")
        {
            Process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!string.IsNullOrEmpty(workDirectory))
                Process.StartInfo.WorkingDirectory = workDirectory;

            if (!string.IsNullOrEmpty(args))
                Process.StartInfo.Arguments = args;
        }

        /// <summary>
        /// 当前运行的Process实例
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// 进程的处理器亲和性
        /// </summary>
        public IntPtr ProcessorAffinity { get; set; } = new IntPtr(0);

        /// <summary>
        /// 输出长度限制
        /// </summary>
        public long OutputLimit { get; set; } = 2684354;

        /// <summary>
        /// 读写流的编码
        /// </summary>
        public Encoding Encoding { get; set; }

        public void Dispose()
        {
            Process.Dispose();
        }

        /// <summary>
        /// 同步运行程序
        /// </summary>
        /// <param name="stdInput">重定向的标准输入</param>
        /// <param name="stdOutput">重定向的标准输出</param>
        /// <param name="stdError">重定向的标准错误</param>
        /// <param name="priorityClass">进程优先级</param>
        /// <param name="inputDelay">输入延迟</param>
        /// <returns>进程退出码</returns>
        public int Run(string stdInput, out string stdOutput, out string stdError,
            ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal,
            int inputDelay = 0)
        {
            Process.Start();
            Process.PriorityClass = priorityClass;

            // 设置进程的处理器亲和性
            if (ProcessorAffinity.ToInt64() != 0)
                Process.ProcessorAffinity = ProcessorAffinity;

            Task<string> readOutputTask = new Task<string>(
                TryReadStreamToEnd, Process.StandardOutput,
                TaskCreationOptions.LongRunning);
            readOutputTask.Start();

            Task<string> readErrorTask = new Task<string>(
                TryReadStreamToEnd, Process.StandardError,
                TaskCreationOptions.LongRunning);
            readErrorTask.Start();

            Thread.Sleep(inputDelay);

            TryWriteToStream(Process.StandardInput, stdInput);

            // 等待读取完成
            Task.WaitAll(readOutputTask, readErrorTask);
            Process.WaitForExit();

            stdOutput = readOutputTask.Result;
            stdError = readErrorTask.Result;

            return Process.ExitCode;
        }

        /// <summary>
        /// 尝试调用StreamWriter写入数据
        /// </summary>
        private void TryWriteToStream(StreamWriter writer, string data)
        {
            try
            {
                writer.Write(data);
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// 尝试调用StreamReader读取到流尾
        /// </summary>
        private string TryReadStreamToEnd(object readerObject)
        {
            StreamReader reader = (StreamReader) readerObject;
            if (Encoding != null)
                reader = new StreamReader(reader.BaseStream, Encoding);

            while (true)
            {
                try
                {
                    return ReadStreamToEnd(reader);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

                Thread.Sleep(ConstReadSleepInterval);
            }
        }

        /// <summary>
        /// 调用StreamReader读取到流尾
        /// </summary>
        private string ReadStreamToEnd(StreamReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char[] buffer = new char[4096];

            // 记录总输出长度
            int sumLength = 0;

            int len;
            while ((len = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                builder.Append(buffer, 0, len);

                sumLength += len;
                if (sumLength > OutputLimit)
                {
                    reader.Close();
                    return builder.ToString();
                }
            }

            reader.Close();
            return builder.ToString();
        }
    }
}