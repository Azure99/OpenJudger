using System;
using System.Collections.Generic;
using System.Text;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Judger.Compilers
{
    /// <summary>
    /// 通用编译器
    /// </summary>
    public class Compiler : ICompiler
    {
        public JudgeTask JudgeTask { get; set; }

        public Compiler(JudgeTask task)
        {
            JudgeTask = task;
        }

        public string Compile()
        {
            using (ProcessRunner runner = new ProcessRunner(
                                              JudgeTask.LangConfig.CompilerPath, 
                                              JudgeTask.LangConfig.CompilerWorkDirectory, 
                                              JudgeTask.LangConfig.CompilerArgs))
            {
                runner.ProcessorAffinity = JudgeTask.ProcessorAffinity;
                if(JudgeTask.LangConfig.UseUTF8)
                {
                    runner.Encoding = Encoding.UTF8;
                }

                RuntimeMonitor monitor = new RuntimeMonitor(runner.Process, 50);
                monitor.TimeLimit = JudgeTask.LangConfig.MaxCompileTime;

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
