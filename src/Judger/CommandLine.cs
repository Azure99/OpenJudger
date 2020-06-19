using System;

namespace Judger
{
    public static class CommandLine
    {
        public static void Loop()
        {
            string command = Console.ReadLine()?.ToLower();

            while (command != null && command != "exit")
            {
                Execute(command);
                command = Console.ReadLine()?.ToLower();
            }

            Console.WriteLine("Bye!");
        }

        private static void Execute(string cmd)
        {
            if (cmd == "status")
                ShowStatus();
            else if (cmd == "clear")
                ClearConsole();
            else if (cmd == "help")
                ShowHelp();
            else
                WrongCommand();
        }

        private static void ShowStatus()
        {
            Console.WriteLine("Service working:\t" + Startup.Service.Working);
            Console.WriteLine("In queue:\t" + Startup.Service.Controller.PendingCount);
            Console.WriteLine("Running:\t" + Startup.Service.Controller.RunningCount);
        }

        private static void ClearConsole()
        {
            Console.Clear();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Status\tShow judge status");
            Console.WriteLine("Clear\tClear console");
            Console.WriteLine("Exit\tShutdown judger");
        }

        private static void WrongCommand()
        {
            Console.WriteLine("Wrong command! Use help to show command list.");
        }
    }
}