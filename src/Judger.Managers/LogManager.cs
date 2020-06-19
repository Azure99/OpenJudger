using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Judger.Models.Exception;

namespace Judger.Managers
{
    /// <summary>
    /// 日志管理器
    /// </summary>
    public static class LogManager
    {
        private const int ConstInfoBufferSize = 512;

        private const string ConstLogLevelInfo = "Info";
        private const string ConstLogLevelDebug = "Debug";
        private const string ConstLogLevelWarning = "Warning";
        private const string ConstLogLevelError = "Error";

        private const string ConstLogDateFormat = "HH:mm:ss dd/MM/yyyy";
        private const string ConstLogFileDateFormat = "yyyyMMdd";

        private const string ConstLogFileInfoPostfix = "Info.txt";
        private const string ConstLogFileDebugPostfix = "Debug.txt";

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
            Log(ConstLogLevelDebug, message);
        }

        public static void Message(string message)
        {
            Console.WriteLine(message);
            Log(ConstLogLevelInfo, message);
        }

        public static void Info(string message)
        {
            Log(ConstLogLevelInfo, message);
        }

        public static void Warning(string message)
        {
            Console.WriteLine(message);
            Log(ConstLogLevelWarning, message);
        }

        public static void Error(string message)
        {
            Console.Error.WriteLine(message);
            Log(ConstLogLevelError, message);
        }

        public static void Exception(Exception originEx, bool showDetails = true)
        {
            Exception ex = originEx;
            string originType = ex.GetType().FullName;
            string originMessage = ex.Message;

            StringBuilder builder = new StringBuilder();
            while (ex != null)
            {
                builder.AppendLine("[" + ex.GetType().FullName + "]");
                builder.AppendLine("Message: " + ex.Message);

                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    builder.AppendLine("Stack Trace: ");
                    builder.AppendLine(ex.StackTrace);
                }

                if (ex.InnerException != null)
                    builder.Append("-->Caused by: ");

                ex = ex.InnerException;
            }

            string content = builder.ToString();

            if (showDetails)
                Console.Error.WriteLine(content);
            else
            {
                Console.Error.WriteLine("[" + originType + "]");
                Console.Error.WriteLine("Message: " + originMessage);
            }

            Log(ConstLogLevelError, content);
            ShowHumanFriendlyMessage(originEx);
        }

        private static void Log(string level, string content)
        {
            string time = DateTime.Now.ToString(ConstLogDateFormat);

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("[{0}] {1}", level, time);
            builder.AppendLine();
            builder.AppendLine(content);
            builder.AppendLine("-");

            content = builder.ToString();

            if (level == ConstLogLevelInfo)
            {
                lock (InfoBuffer)
                {
                    InfoBuffer.Append(content);

                    if (InfoBuffer.Length > ConstInfoBufferSize)
                        FlushBuffer();
                }

                return;
            }

            WriteLog(level, content);
        }

        public static void FlushBuffer()
        {
            lock (BufferLock)
            {
                WriteLog(ConstLogLevelInfo, InfoBuffer.ToString());
                InfoBuffer.Clear();
            }
        }

        public static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception(e.ExceptionObject as Exception);
        }

        private static void ShowHumanFriendlyMessage(Exception ex)
        {
            while (ex != null)
            {
                if (ex is BaseException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("--↓ Exception Hint ↓--");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("--↑ Exception Hint ↑--" + Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                }

                ex = ex.InnerException;
            }
        }

        private static void WriteLog(string level, string content)
        {
            string fileName = GetLogFileName(level);
            string path = Path.Combine(ConfigManager.Config.LogDirectory, fileName);

            try
            {
                lock (WriteLock)
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
            string date = DateTime.Now.ToString(ConstLogFileDateFormat);

            return level switch
            {
                ConstLogLevelInfo => date + "-" + ConstLogFileInfoPostfix,
                ConstLogLevelDebug => date + "-" + ConstLogFileDebugPostfix,
                _ => date + ".txt"
            };
        }

        private static void AutoFlush()
        {
            while (true)
            {
                FlushBuffer();
                Thread.Sleep(5000);
            }
        }
    }
}