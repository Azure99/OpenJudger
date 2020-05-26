using System;
using System.Text;
using Judger.Models;
using Judger.Models.Judge;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Core.Program.Internal
{
    /// <summary>
    /// 程序编译器
    /// </summary>
    public class Compiler
    {
        public Compiler(JudgeContext context)
        {
            JudgeTask = context.Task;
            LangConfig = context.LangConfig as ProgramLangConfig;
        }

        private JudgeTask JudgeTask { get; }
        private ProgramLangConfig LangConfig { get; }

        /// <summary>
        /// 编译评测任务的代码
        /// </summary>
        /// <returns>信息(不为空则有编译错误)</returns>
        public string Compile()
        {
            using (ProcessRunner runner = CreateProcessRunner())
            {
                runner.ProcessorAffinity = JudgeTask.ProcessorAffinity;
                if (LangConfig.UseUtf8)
                    runner.Encoding = Encoding.UTF8;

                RuntimeMonitor monitor = new RuntimeMonitor(runner.Process, 50)
                {
                    TimeLimit = LangConfig.MaxCompileTime,
                    TotalTimeLimit = LangConfig.MaxCompileTime
                };

                monitor.Start();

                int exitCode;
                string error;

                try
                {
                    exitCode = runner.Run("", out string _, out error);
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
                finally
                {
                    monitor.Dispose();
                }

                if (exitCode != 0)
                {
                    if (monitor.LimitExceed)
                        return "Compile timeout.";

                    if (string.IsNullOrEmpty(error))
                        return "Compiler runtime error.";

                    return error;
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