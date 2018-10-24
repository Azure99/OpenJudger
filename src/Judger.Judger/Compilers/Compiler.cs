using System;
using System.Collections.Generic;
using System.Text;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Judger.Compilers
{
    /// <summary>
    /// 通用编译器
    /// </summary>
    public class Compiler : ICompiler
    {
        public string CompilerPath { get; set; }

        public string CompilerWorkDirectory { get; set; }

        public int TimeLimit { get; set; }

        public IntPtr ProcessorAffinity { get; set; } = ProcessorAffinityManager.DefaultAffinity;

        public Compiler(string compilerPath, string compilerWorkDirectory)
        {
            CompilerPath = compilerPath;
            CompilerWorkDirectory = compilerWorkDirectory;
        }

        public string Compile(string args)
        {
            using (ProcessRunner runner = new ProcessRunner(CompilerPath, CompilerWorkDirectory, args))
            {
                runner.ProcessorAffinity = ProcessorAffinity;

                RuntimeMonitor monitor = new RuntimeMonitor(runner.Process);
                monitor.TimeLimit = TimeLimit;

                monitor.Start();

                int exitcode;
                string output, error;

                try
                {
                    exitcode = runner.Run("", out output, out error);
                }
                catch(Exception e)
                {
                    return e.ToString();
                }
                finally
                {
                    monitor.Dispose();
                }

                if(!string.IsNullOrEmpty(error)) //存在编译错误
                {
                    return error;
                }

                if(exitcode != 0)
                {
                    if(monitor.LimitExceed)
                    {
                        return "Compile timeout.";
                    }
                    return "Compiler runtime error.";
                }

                return "";
            }
        }
    }
}
