using System;
using Judger.Service;

namespace Judger
{
    class Program
    {
        private static JudgeService Service;

        static void Main(string[] args)
        {
            Console.WriteLine("--- Open Judger ---");
            Console.WriteLine("Starting judge service...");
            StartUp();
            Console.WriteLine("All done!");

            ReadCommand();
        }

        private static void StartUp()
        {
            Service = new JudgeService();
            Service.Start();
        }
        
        private static void ReadCommand()
        {
            string command = Console.ReadLine();
            while (true)
            {
                command = command.ToLower();
                if (command == "exit") //退出
                {
                    break;
                }
                else if (command == "status") //显示当前状态
                {
                    Console.WriteLine("Service working: " + Service.Working);
                    Console.WriteLine("In queue: " + Service.JudgeManager.InQueueCount);
                    Console.WriteLine("Running: " + Service.JudgeManager.RunningCount);
                }
                else if(command == "clear") //清屏
                {
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Wrong command!");
                }
                command = Console.ReadLine();
            }

            Console.WriteLine("Bye!");
        }
    }
}
