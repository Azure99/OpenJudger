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
        private const int INFO_BUFFER_SIZE = 512;

        private const string LOG_LEVEL_INFO = "Info";
        private const string LOG_LEVEL_DEBUG = "Debug";
        private const string LOG_LEVEL_WARNING = "Warning";
        private const string LOG_LEVEL_ERROR = "Error";

        private const string LOG_DATE_FORMAT = "HH:mm:ss dd/MM/yyyy";
        private const string LOG_FILE_DATE_FORMAT = "yyyyMMdd";

        private const string LOG_FILE_INFO_POSTFIX = "Info.txt";
        private const string LOG_FILE_DEBUG_POSTFIX = "Debug.txt";

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

        static LogManager()
        {
            if (!Directory.Exists(ConfigManager.Config.LogDirectory))
                Directory.CreateDirectory(ConfigManager.Config.LogDirectory);

            _autoFlushTask.Start();
        }

        /// <summary>
        /// 输出Debug级日志
        /// </summary>
        public static void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            Log(LOG_LEVEL_DEBUG, message);
        }

        /// <summary>
        /// 输出Info级日志
        /// </summary>
        public static void Info(string message)
        {
            Log(LOG_LEVEL_INFO, message);
        }

        /// <summary>
        /// 输出Waring级日志
        /// </summary>
        public static void Warning(string message)
        {
            Log(LOG_LEVEL_WARNING, message);
        }

        /// <summary>
        /// 输出Error级日志
        /// </summary>
        public static void Error(string message)
        {
            Console.Error.WriteLine(message);
            Log(LOG_LEVEL_ERROR, message);
        }

        /// <summary>
        /// 输出Exception(Error)级
        /// </summary>
        public static void Exception(Exception ex, bool showDetails = true)
        {
            string originalType = ex.GetType().FullName;
            string originalMessage = ex.Message;

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
                    sb.Append("-->Caused by:");

                ex = ex.InnerException;
            }

            string content = sb.ToString();

            if (showDetails)
            {
                Console.Error.WriteLine(content);
            }
            else
            {
                Console.Error.WriteLine("[" + originalType + "]");
                Console.Error.WriteLine("Message:" + originalMessage);
            }

            Log(LOG_LEVEL_ERROR, content);
        }

        private static void Log(string level, string content)
        {
            string time = DateTime.Now.ToString(LOG_DATE_FORMAT);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}] {1}", level, time);
            sb.AppendLine();
            sb.AppendLine(content);
            sb.AppendLine("-");

            content = sb.ToString();

            if (level == LOG_LEVEL_INFO)
            {
                lock (_infoBuffer)
                {
                    _infoBuffer.Append(content);
                }

                if (_infoBuffer.Length > INFO_BUFFER_SIZE)
                    Flush();

                return;
            }

            Write(level, content);
        }

        public static void Flush()
        {
            lock (_bufferLock)
            {
                Write(LOG_LEVEL_INFO, _infoBuffer.ToString());
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
            string date = DateTime.Now.ToString(LOG_FILE_DATE_FORMAT);

            if (level == LOG_LEVEL_INFO)
                return date + "-" + LOG_FILE_INFO_POSTFIX;

            if (level == LOG_LEVEL_DEBUG)
                return date + "-" + LOG_FILE_DEBUG_POSTFIX;

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