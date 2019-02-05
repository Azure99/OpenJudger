using System;
using System.Text;
using Judger.Entity;
using Judger.Utils;

namespace Judger.Core.Program
{
    /// <summary>
    /// 程序编译器
    /// </summary>
    public class Compiler
    {
        public JudgeTask JudgeTask { get; set; }

        public Compiler(JudgeTask task)
        {
            JudgeTask = task;
        }

        /// <summary>
        /// 编译评测任务的代码
        /// </summary>
        /// <returns></returns>
        public string Compile()
        {
            using (ProcessRunner runner = CreateProcessRunner())
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
                catch(Exception ex)
                {
                    return ex.ToString();
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

        private ProcessRunner CreateProcessRunner()
        {
            return new ProcessRunner(
                JudgeTask.LangConfig.CompilerPath,
                JudgeTask.LangConfig.CompilerWorkDirectory,
                JudgeTask.LangConfig.CompilerArgs);
        }
    }
}
