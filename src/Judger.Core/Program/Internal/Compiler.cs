using System;
using System.Text;
using Judger.Entity;
using Judger.Entity.Program;
using Judger.Utils;

namespace Judger.Core.Program.Internal
{
    /// <summary>
    /// 程序编译器
    /// </summary>
    public class Compiler
    {
        public JudgeTask JudgeTask { get; set; }

        public ProgramLangConfig LangConfig { get; private set; }

        public Compiler(JudgeTask task)
        {
            JudgeTask = task;
            LangConfig = JudgeTask.LangConfig as ProgramLangConfig;
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
                if (LangConfig.UseUTF8)
                {
                    runner.Encoding = Encoding.UTF8;
                }

                RuntimeMonitor monitor = new RuntimeMonitor(runner.Process, 50);
                monitor.TimeLimit = LangConfig.MaxCompileTime;
                monitor.TotalTimeLimit = LangConfig.MaxCompileTime;

                monitor.Start();

                int exitcode;
                string output, error;

                try
                {
                    exitcode = runner.Run("", out output, out error);
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
                finally
                {
                    monitor.Dispose();
                }

                if (exitcode != 0)
                {
                    if (monitor.LimitExceed)
                    {
                        return "Compile timeout.";
                    }

                    if (string.IsNullOrEmpty(error))
                    {
                        return "Compiler runtime error.";
                    }
                    else
                    {
                        return error;
                    }
                }

                return "";
            }
        }

        private ProcessRunner CreateProcessRunner()
        {
            return new ProcessRunner(
                LangConfig.CompilerPath,
                LangConfig.CompilerWorkDirectory,
                LangConfig.CompilerArgs);
        }
    }
}