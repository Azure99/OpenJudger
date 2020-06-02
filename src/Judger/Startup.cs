using System;
using System.IO;
using Judger.Adapter;
using Judger.Managers;
using Judger.Service;
using Judger.Utils;

namespace Judger
{
    public static class Startup
    {
        public static JudgeService Service { get; private set; }

        public static void Run(string[] args)
        {
            CheckInit();

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

        private static void CheckInit()
        {
            if (File.Exists("Config.json"))
                return;

            IConfigInitializer[] initializers = AdapterFactory.GetConfigInitializers();
            if (initializers.Length <= 0)
                return;
            
            Console.WriteLine("Welcome to use OpenJudger!");
            Console.WriteLine("Run config initializer? (y/n)");
            string input = Console.ReadLine();
            if (input.ToLower() != "y")
            {
                Console.WriteLine("If you want to run initializer again, " +
                                  "please delete the Config.json file and restart OpenJudger.");
                return;
            }
            
            Console.WriteLine("Select initializer (number):");
            for (int i = 0; i < initializers.Length; i++)
            {
                string name = initializers[i].GetType().FullName.TrimStart("Judger.Adapter.");
                Console.WriteLine($"{i}) {name}");
            }

            if (!int.TryParse(Console.ReadLine(), out int id) || id < 0 || id >= initializers.Length)
            {
                Console.WriteLine("Input error, please input the number of initializer!");
                if (File.Exists("Config.json"))
                    File.Delete("Config.json");
                Environment.Exit(0);
            }
            
            initializers[id].Init();
            Console.WriteLine("OK, Please restart OpenJudger!");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}