using System;

namespace Judger
{
    public class CommandLine
    {
        /// <summary>
        /// 开始读取CLI命令
        /// </summary>
        public static void Read()
        {
            string command = Console.ReadLine();
            while (true)
            {
                command = command.ToLower();
                if (command == "exit") // 退出
                    break;

                if (command == "status") // 显示当前状态
                {
                    Console.WriteLine("Service working:\t" + Startup.Service.Working);
                    Console.WriteLine("In queue:\t" + Startup.Service.Controller.InQueueCount);
                    Console.WriteLine("Running:\t" + Startup.Service.Controller.RunningCount);
                }
                else if (command == "clear") // 清屏
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