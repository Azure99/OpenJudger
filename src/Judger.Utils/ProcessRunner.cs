using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Judger.Utils
{
    /// <summary>
    /// 运行指定程序并重定向输入输出流, 支持输出限制
    /// </summary>
    public class ProcessRunner : IDisposable
    {
        /// <summary>
        /// 当前运行的Process实例
        /// </summary>
        public Process Process { get; }

        /// <summary>
        /// Process的处理器亲和性
        /// </summary>
        public IntPtr ProcessorAffinity { get; set; } = new IntPtr(0);

        /// <summary>
        /// 输出长度限制
        /// </summary>
        public long OutputLimit { get; set; } = 2684354;

        /// <summary>
        /// 程序运行器
        /// </summary>
        /// <param name="fileName">程序文件名</param>
        /// <param name="workDirectory">工作目录</param>
        /// <param name="args">运行参数</param>
        public ProcessRunner(string fileName, string workDirectory = "", string args = "")
        {
            Process = new Process();

            Process.StartInfo.FileName = fileName;
            if (!string.IsNullOrEmpty(workDirectory))
            {
                Process.StartInfo.WorkingDirectory = workDirectory;
            }
            if (!string.IsNullOrEmpty(args))
            {
                Process.StartInfo.Arguments = args;
            }
            
            Process.StartInfo.RedirectStandardInput = true;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.RedirectStandardError = true;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.CreateNoWindow = true;
        }

        /// <summary>
        /// 同步运行程序
        /// </summary>
        /// <param name="stdInput">重定向的标准输入</param>
        /// <param name="stdOutput">重定向的标准输出</param>
        /// <param name="stdError">重定向的标准错误</param>
        /// <param name="priorityClass">进程优先级</param>
        /// <returns>进程退出码</returns>
        public int Run(string stdInput, out string stdOutput, out string stdError, 
                       ProcessPriorityClass priorityClass = ProcessPriorityClass.Normal)
        {
            Process.Start();
            Process.PriorityClass = priorityClass;

            // 设置进程的处理器亲和性
            if (ProcessorAffinity.ToInt64() != 0)
            {
                Process.ProcessorAffinity = ProcessorAffinity;
            }

            Task<string> readOutputTask = new Task<string>(
                             TryReadStreamToEnd, Process.StandardOutput, 
                             TaskCreationOptions.LongRunning);
            readOutputTask.Start();

            Task<string> readErrorTask = new Task<string>(
                             TryReadStreamToEnd, Process.StandardError, 
                             TaskCreationOptions.LongRunning);
            readErrorTask.Start();

            try
            {
                Process.StandardInput.Write(stdInput);
                Process.StandardInput.Flush();
                Process.StandardInput.Close();
            }
            catch { }

            //等待读取完成
            Task.WaitAll(readOutputTask, readErrorTask);
            Process.WaitForExit();

            stdOutput = readOutputTask.Result;
            stdError = readErrorTask.Result;

            return Process.ExitCode;
        }

        public void Dispose()
        {
            Process.Dispose();
        }

        /// <summary>
        /// 尝试调用StreamReader读取到流尾
        /// </summary>
        /// <param name="readerObject">StreamReader对象</param>
        /// <returns>结果</returns>
        private string TryReadStreamToEnd(object readerObject)
        {
            StreamReader reader = readerObject as StreamReader;
            // ConsoleEncoding读取中文会乱码，使用UTF8编码
            reader = new StreamReader(reader.BaseStream, Encoding.UTF8); 

            while (true)
            {
                try
                {
                    return ReadStreamToEnd(reader);
                }
                catch { }
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// 调用StreamReader读取到流尾
        /// </summary>
        /// <param name="reader">StreamReader</param>
        /// <returns>结果</returns>
        private string ReadStreamToEnd(StreamReader reader)
        {
            StringBuilder sb = new StringBuilder();
            char[] buffer = new char[4096];

            // 记录总输出长度
            int sumLength = 0;

            int len = 0;
            while ((len = reader.Read(buffer, 0, buffer.Length)) > 0) 
            {
                sb.Append(buffer, 0, len);

                sumLength += len;
                if (sumLength > OutputLimit)//检查输出超限
                {
                    reader.Close();
                    return sb.ToString();
                }
            }

            reader.Close();
            return sb.ToString();
        }
    }
}
