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

        private static readonly StringBuilder InfoBuffer = new StringBuilder();
        private static readonly object BufferLock = new object();
        private static readonly object WriteLock = new object();

        private static readonly Task AutoFlushTask = new Task(AutoFlush, TaskCreationOptions.LongRunning);

        static LogManager()
        {
            if (!Directory.Exists(ConfigManager.Config.LogDirectory))
                Directory.CreateDirectory(ConfigManager.Config.LogDirectory);

            AutoFlushTask.Start();
        }

        public static void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
            Log(LOG_LEVEL_DEBUG, message);
        }

        public static void Info(string message)
        {
            Log(LOG_LEVEL_INFO, message);
        }

        public static void Warning(string message)
        {
            Log(LOG_LEVEL_WARNING, message);
        }

        public static void Error(string message)
        {
            Console.Error.WriteLine(message);
            Log(LOG_LEVEL_ERROR, message);
        }

        public static void Exception(Exception ex, bool showDetails = true)
        {
            string originalType = ex.GetType().FullName;
            string originalMessage = ex.Message;

            StringBuilder builder = new StringBuilder();
            while (ex != null)
            {
                builder.AppendLine("[" + ex.GetType().FullName + "]");
                builder.AppendLine("Message:" + ex.Message);

                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    builder.AppendLine("Stack Trace:");
                    builder.AppendLine(ex.StackTrace);
                }

                if (ex.InnerException != null)
                    builder.Append("-->Caused by:");

                ex = ex.InnerException;
            }

            string content = builder.ToString();

            if (showDetails)
                Console.Error.WriteLine(content);
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

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("[{0}] {1}", level, time);
            builder.AppendLine();
            builder.AppendLine(content);
            builder.AppendLine("-");

            content = builder.ToString();

            if (level == LOG_LEVEL_INFO)
            {
                lock (InfoBuffer)
                {
                    InfoBuffer.Append(content);

                    if (InfoBuffer.Length > INFO_BUFFER_SIZE)
                        Flush();
                }

                return;
            }

            Write(level, content);
        }

        public static void Flush()
        {
            lock (BufferLock)
            {
                Write(LOG_LEVEL_INFO, InfoBuffer.ToString());
                InfoBuffer.Clear();
            }
        }

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
                lock (WriteLock)
                    File.AppendAllText(path, content);
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