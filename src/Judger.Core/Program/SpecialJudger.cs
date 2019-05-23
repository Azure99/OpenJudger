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
            JudgeTask.ProcessorAffinity = ProcessorAffinityManager.GetUseage();
            LangConfig = context.LangConfig as ProgramLangConfig;
            SpjContext = SpjManager.CreateSpjJudgeContext(context);
            SpjTask = SpjContext.Task;
            SpjLangConfig = SpjContext.LangConfig as ProgramLangConfig;
        }
        
        private JudgeContext SpjContext { get; set; }
        
        /// <summary>
        /// 注意:未编写:SPJTasks中的语言应对应SPJ程序，而不是JudgeTask
        /// </summary>
        private JudgeTask SpjTask { get; set; }

        private ProgramLangConfig SpjLangConfig { get; set; }

        private ProgramLangConfig LangConfig { get; set; }

        public override void Judge()
        {
            //判题结果
            JudgeResult result = Context.Result;

            //正则恶意代码检查
            if (!CodeChecker.Singleton.CheckCode(
                JudgeTask.SourceCode, JudgeTask.Language,
                out string unsafeCode, out int line))
            {
                result.ResultCode = JudgeResultCode.CompileError;
                result.JudgeDetail = "Include unsafe code, please remove them!";
                result.JudgeDetail += "\r\n";
                result.JudgeDetail += "line " + line + ": " + unsafeCode;
                return;
            }

            //构建SPJ程序
            BuildSpecialJudgeProgram();

            //写出源代码
            string sourceFileName = Path.Combine(Context.TempDirectory + LangConfig.SourceCodeFileName);
            File.WriteAllText(sourceFileName, JudgeTask.SourceCode);

            //编译代码
            if (LangConfig.NeedCompile)
            {
                Compiler compiler = new Compiler(Context);
                string compileRes = compiler.Compile();

                //检查是否有编译错误(compileRes不为空则代表有错误)
                if (!string.IsNullOrEmpty(compileRes))
                {
                    //去除路径信息
                    result.JudgeDetail = compileRes.Replace(Context.TempDirectory, "");
                    result.ResultCode = JudgeResultCode.CompileError;
                    result.MemoryCost = 0;
                    return;
                }
            }

            //创建单例Judger
            SpecialSingleCaseJudger judger = new SpecialSingleCaseJudger(Context, SpjContext);

            //获取所有测试点文件名
            Tuple<string, string>[] dataFiles = TestDataManager.GetTestDataFilesName(JudgeTask.ProblemId);
            if (dataFiles.Length == 0) //无测试数据
            {
                result.ResultCode = JudgeResultCode.JudgeFailed;
                result.JudgeDetail = "No test data.";
                return;
            }

            int acceptedCasesCount = 0; //通过的测试点数
            for (int i = 0; i < dataFiles.Length; i++)
            {
                try
                {
                    //读入测试数据
                    TestDataManager.GetTestData(
                        JudgeTask.ProblemId, dataFiles[i].Item1, dataFiles[i].Item2,
                        out string input, out string output);

                    SingleJudgeResult singleRes = judger.Judge(input, output); //测试此测试点

                    //计算有时间补偿的总时间
                    result.TimeCost = Math.Max(result.TimeCost,
                        (int) (singleRes.TimeCost * LangConfig.TimeCompensation));
                    result.MemoryCost = Math.Max(result.MemoryCost, singleRes.MemoryCost);

                    if (singleRes.ResultCode == JudgeResultCode.Accepted)
                    {
                        acceptedCasesCount++;
                    }
                    else
                    {
                        result.ResultCode = singleRes.ResultCode;
                        result.JudgeDetail = singleRes.JudgeDetail;

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

            //去除目录信息
            result.JudgeDetail = result.JudgeDetail.Replace(Context.TempDirectory, "");

            //通过率
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

            if (LangConfig.NeedCompile)
            {
                //构建SPJ程序
                if (TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId) == null)
                {
                    if (!CompileSpecialJudgeProgram())
                        throw new CompileException("Can not build special judge program!");
                }

                SpecialJudgeProgram spjProgram = TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId);
                File.WriteAllBytes(
                    Path.Combine(SpjContext.TempDirectory, spjProgram.LangConfiguration.ProgramFileName),
                    spjProgram.Program);
            }
        }

        /// <summary>
        /// 编译SPJ程序
        /// </summary>
        /// <returns>是否编译并写出成功</returns>
        private bool CompileSpecialJudgeProgram()
        {
            Compiler compiler = new Compiler(SpjContext);
            string compileResult = compiler.Compile();
            if (compileResult != "")
                throw new CompileException("Can not compile special judge program!" + Environment.NewLine +
                                           compileResult);

            string spjProgramPath =
                Path.Combine(SpjContext.TempDirectory, LangConfig.ProgramFileName);

            if (!File.Exists(spjProgramPath))
                throw new CompileException("Special judge program not found!");

            SpecialJudgeProgram spjProgram = new SpecialJudgeProgram
            {
                LangConfiguration = LangConfig,
                Program = File.ReadAllBytes(spjProgramPath)
            };

            TestDataManager.WriteSpecialJudgeProgramFile(JudgeTask.ProblemId, spjProgram);

            return TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemId) != null;
        }

        public override void Dispose()
        {
            ProcessorAffinityManager.ReleaseUseage(JudgeTask.ProcessorAffinity);
            DeleteTempDirectory();
        }

        /// <summary>
        /// 删除临时目录
        /// </summary>
        private void DeleteTempDirectory()
        {
            new Task(() => //判题结束时文件可能仍然被占用，尝试删除
            {
                int tryCount = 0;
                while (true)
                {
                    try
                    {
                        Directory.Delete(Context.TempDirectory, true); //删除判题临时目录
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