using System;
using Judger.Managers;
using Judger.Service;

namespace Judger
{
    public static class Startup
    {
        public static JudgeService Service { get; private set; }

        public static void Run(string[] args)
        {
            Console.WriteLine("--- Open Judger ---");
            Console.WriteLine("Starting judge service...");
            StartUp();
            Console.WriteLine("All done!");

            CommandLine.Loop();
        }

        private static void StartUp()
        {
            SetAppHandle();
            LogManager.Info("Starting judger");

            Service = new JudgeService();
            Service.Start();

            LogManager.Info("Judger started");
        }

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