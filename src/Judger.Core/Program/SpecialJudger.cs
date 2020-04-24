using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Judger.Core.Program.Internal;
using Judger.Core.Program.Internal.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;
using Judger.Models.Program;

namespace Judger.Core.Program
{
    public class SpecialJudger : BaseJudger
    {
        public SpecialJudger(JudgeContext context) : base(context)
        {
            JudgeTask.ProcessorAffinity = ProcessorAffinityManager.GetUsage();
            LangConfig = context.LangConfig as ProgramLangConfig;
            SpjContext = SpjManager.CreateSpjJudgeContext(context);
            SpjTask = SpjContext.Task;
            SpjLangConfig = SpjContext.LangConfig as ProgramLangConfig;
        }

        private JudgeContext SpjContext { get; }

        /// <summary>
        /// 注意:未编写:SPJTasks中的语言应对应SPJ程序，而不是JudgeTask
        /// </summary>
        private JudgeTask SpjTask { get; }

        private ProgramLangConfig SpjLangConfig { get; }

        private ProgramLangConfig LangConfig { get; }

        public override void Judge()
        {
            JudgeResult result = Context.Result;

            if (!CodeChecker.Instance.CheckCode(
                JudgeTask.SourceCode, JudgeTask.Language,
                out string unsafeCode, out int line))
            {
                result.ResultCode = JudgeResultCode.CompileError;
                result.JudgeDetail = "Include unsafe code, please remove them!";
                result.JudgeDetail += "\r\n";
                result.JudgeDetail += "line " + line + ": " + unsafeCode;
                return;
            }

            // 构建SPJ程序
            BuildSpecialJudgeProgram();

            string sourceFileName = Path.Combine(Context.TempDirectory + LangConfig.SourceCodeFileName);
            File.WriteAllText(sourceFileName, JudgeTask.SourceCode);

            if (LangConfig.NeedCompile)
            {
                var compiler = new Compiler(Context);
                string compileRes = compiler.Compile();

                if (!string.IsNullOrEmpty(compileRes))
                {
                    // 去除路径信息
                    result.JudgeDetail = compileRes.Replace(Context.TempDirectory, "");
                    result.ResultCode = JudgeResultCode.CompileError;
                    result.MemoryCost = 0;
                    return;
                }
            }

            var judger = new SpecialSingleCaseJudger(Context, SpjContext);

            ProgramTestDataFile[] dataFiles = TestDataManager.GetTestDataFilesName(JudgeTask.ProblemId);
            if (dataFiles.Length == 0)
            {
                result.ResultCode = JudgeResultCode.JudgeFailed;
                result.JudgeDetail = "No test data.";
                return;
            }

            var acceptedCasesCount = 0;
            for (var i = 0; i < dataFiles.Length; i++)
            {
                try
                {
                    ProgramTestData data = TestDataManager.GetTestData(JudgeTask.ProblemId, dataFiles[i]);

                    SingleJudgeResult singleRes = judger.Judge(data.Input, data.Output);

                    if (result.ResultCode == JudgeResultCode.Accepted)
                    {

                        result.TimeCost = Math.Max(result.TimeCost,
                            (int) (singleRes.TimeCost * LangConfig.TimeCompensation));
                        result.MemoryCost = Math.Max(result.MemoryCost, singleRes.MemoryCost);
                    }

                    if (singleRes.ResultCode == JudgeResultCode.Accepted)
                    {
                        acceptedCasesCount++;
                    }
                    else
                    {
                        if (result.ResultCode == JudgeResultCode.Accepted)
                        {
                            result.ResultCode = singleRes.ResultCode;
                            result.JudgeDetail = singleRes.JudgeDetail;
                        }

                        if (!JudgeTask.JudgeAllCases)
                            break;
                    }
                }
                catch (Exception e)
                {
                    result.ResultCode = JudgeResultCode.JudgeFailed;
                    result.JudgeDetail = e.ToString();
                    break;
                }
            }

            result.JudgeDetail = result.JudgeDetail.Replace(Context.TempDirectory, "");
            result.PassRate = (double) acceptedCasesCount / dataFiles.Length;
        }

        /// <summary>
        /// 构建并写出供Judger使用的SPJ程序
        /// </summary>
        private void BuildSpecialJudgeProgram()
        {
            File.WriteAllText(
                Path.Combine(SpjContext.TempDirectory, SpjLangConfig.SourceCodeFileName),
                SpjTask.SourceCode);

            if (!LangConfig.NeedCompile)
                return;

            // 构建SPJ程序
            if (TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId) == null)
                if (!CompileSpecialJudgeProgram())
                    throw new CompileException("Can not build special judge program!");

            SpecialJudgeProgram spjProgram = TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId);
            File.WriteAllBytes(
                Path.Combine(SpjContext.TempDirectory, spjProgram.LangConfiguration.ProgramFileName),
                spjProgram.Program);
        }

        /// <summary>
        /// 编译SPJ程序
        /// </summary>
        /// <returns>是否编译并写出成功</returns>
        private bool CompileSpecialJudgeProgram()
        {
            var compiler = new Compiler(SpjContext);
            string compileResult = compiler.Compile();
            if (compileResult != "")
                throw new CompileException("Can not compile special judge program!" + Environment.NewLine +
                                           compileResult);

            string spjProgramPath =
                Path.Combine(SpjContext.TempDirectory, LangConfig.ProgramFileName);

            if (!File.Exists(spjProgramPath))
                throw new CompileException("Special judge program not found!");

            var spjProgram = new SpecialJudgeProgram
            {
                LangConfiguration = LangConfig,
                Program = File.ReadAllBytes(spjProgramPath)
            };

            TestDataManager.WriteSpecialJudgeProgramFile(JudgeTask.ProblemId, spjProgram);

            return TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId) != null;
        }

        public override void Dispose()
        {
            ProcessorAffinityManager.ReleaseUsage(JudgeTask.ProcessorAffinity);
            DeleteTempDirectory();
        }

        private void DeleteTempDirectory()
        {
            new Task(() =>
            {
                var tryCount = 0;
                while (true)
                {
                    try
                    {
                        Directory.Delete(Context.TempDirectory, true);
                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        break;
                    }
                    catch
                    {
                        if (tryCount++ > 20)
                            throw new JudgeException("Cannot delete temp directory");

                        Thread.Sleep(500);
                    }
                }
            }, TaskCreationOptions.LongRunning).Start();
        }
    }
}