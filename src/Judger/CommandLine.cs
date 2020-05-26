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
                Status();
            else if (cmd == "clear")
                Clear();
            else if (cmd == "help")
                Help();
            else
                WrongCommand();
        }

        private static void Status()
        {
            Console.WriteLine("Service working:\t" + Startup.Service.Working);
            Console.WriteLine("In queue:\t" + Startup.Service.Controller.PendingCount);
            Console.WriteLine("Running:\t" + Startup.Service.Controller.RunningCount);
        }

        private static void Clear()
        {
            Console.Clear();
        }

        private static void WrongCommand()
        {
            Console.WriteLine("Wrong command! Use help to show command list.");
        }

        private static void Help()
        {
            Console.WriteLine("Status\tShow judge status");
            Console.WriteLine("Clear\tClear console");
            Console.WriteLine("Exit\tShutdown judger");
        }
    }
}