using System;
using Judger.Managers;
using Judger.Service;

namespace Judger
{
    /// <summary>
    /// Judger启动类
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// 评测服务
        /// </summary>
        public static JudgeService Service { get; private set; }

        /// <summary>
        /// 运行Jugder
        /// </summary>
        /// <param name="args"></param>
        public static void Run(string[] args)
        {
            Console.WriteLine("--- Open Judger ---");
            Console.WriteLine("Starting judge service...");
            StartUp();
            Console.WriteLine("All done!");

            CommandLine.Read();
        }

        /// <summary>
        /// 开始服务
        /// </summary>
        private static void StartUp()
        {
            SetAppHandle();
            LogManager.Info("Starting judger");

            Service = new JudgeService();
            Service.Start();

            LogManager.Info("Judger started");
        }

        /// <summary>
        /// 设置应用Handle
        /// </summary>
        private static void SetAppHandle()
        {
            AppDomain.CurrentDomain.UnhandledException += LogManager.OnUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                LogManager.Info("Judger stopped");
                LogManager.Flush();
            };
        }
    }
}