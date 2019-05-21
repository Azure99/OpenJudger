using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Judger.Managers
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// 信息缓冲区
        /// </summary>
        private static StringBuilder _infoBuffer = new StringBuilder();

        /// <summary>
        /// 缓冲区锁
        /// </summary>
        private static object _bufferLock = new object();

        /// <summary>
        /// 写操作锁
        /// </summary>
        private static object _writeLock = new object();

        /// <summary>
        /// 自动刷新缓冲区任务
        /// </summary>
        private static Task _autoFlushTask = new Task(AutoFlush, TaskCreationOptions.LongRunning);

        // 仅对Info级日志进行缓冲输出
        private const int INFO_BUFFER_SIZE = 512;

        static LogManager()
        {
            if (!Directory.Exists(ConfigManager.Config.LogDirectory))
            {
                Directory.CreateDirectory(ConfigManager.Config.LogDirectory);
            }

            _autoFlushTask.Start();
        }

        /// <summary>
        /// 输出Debug级日志
        /// </summary>
        public static void Debug(string message)
        {
            Log("Debug", message);
        }

        /// <summary>
        /// 输出Info级日志
        /// </summary>
        public static void Info(string message)
        {
            Log("Info", message);
        }

        /// <summary>
        /// 输出Waring级日志
        /// </summary>
        public static void Warning(string message)
        {
            Log("Warning", message);
        }

        /// <summary>
        /// 输出Error级日志
        /// </summary>
        public static void Error(string message)
        {
            Log("Error", message);
        }

        /// <summary>
        /// 输出Exception(Error)级
        /// </summary>
        public static void Exception(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine("[" + ex.GetType().FullName + "]");
                sb.AppendLine("Message:" + ex.Message);

                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    sb.AppendLine("Stack Trace:");
                    sb.AppendLine(ex.StackTrace);
                }

                if (ex.InnerException != null)
                {
                    sb.Append("-->Caused by:");
                }

                ex = ex.InnerException;
            }

            Log("Error", sb.ToString());
        }

        private static void Log(string level, string content)
        {
            if (level == "Error")
            {
                Console.Error.WriteLine(content);
            }
            else if (level == "Debug")
            {
                System.Diagnostics.Debug.WriteLine(content);
            }

            string time = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}] {1}", level, time);
            sb.AppendLine();
            sb.AppendLine(content);
            sb.AppendLine("-");

            content = sb.ToString();

            if (level == "Info")
            {
                lock (_infoBuffer)
                {
                    _infoBuffer.Append(content);
                }

                if (_infoBuffer.Length > INFO_BUFFER_SIZE)
                {
                    Flush();
                }

                return;
            }

            Write(level, content);
        }

        public static void Flush()
        {
            lock (_bufferLock)
            {
                Write("Info", _infoBuffer.ToString());
                _infoBuffer.Clear();
            }
        }

        /// <summary>
        /// 捕获全局未处理的异常
        /// </summary>
        public static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception(e.ExceptionObject as Exception);
        }

        private static void Write(string level, string content)
        {
            string fileName = GetLogFileName(level);
            string path = Path.Combine(ConfigManager.Config.LogDirectory, fileName);

            try
            {
                lock (_writeLock)
                {
                    File.AppendAllText(path, content);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static string GetLogFileName(string level)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");

            if (level == "Info")
            {
                return date + "-" + "Info.txt";
            }
            else if (level == "Debug")
            {
                return date + "-" + "Debug.txt";
            }

            return date + ".txt";
        }

        private static void AutoFlush()
        {
            while (true)
            {
                Flush();
                Thread.Sleep(5000);
            }
        }
    }
}