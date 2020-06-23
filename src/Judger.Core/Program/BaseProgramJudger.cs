using System;
using System.IO;
using Judger.Core.Program.Internal;
using Judger.Core.Program.Internal.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;
using Judger.Models.Program;

namespace Judger.Core.Program
{
    public abstract class BaseProgramJudger : BaseJudger
    {
        protected BaseProgramJudger(JudgeContext context) : base(context)
        {
            JudgeTask.ProcessorAffinity = ProcessorAffinityManager.GetUsage();
            LangConfig = context.LangConfig as ProgramLangConfig;
        }

        protected ProgramLangConfig LangConfig { get; }

        protected void CheckUnsafeCode()
        {
            JudgeResult result = JudgeResult;
            if (!CodeChecker.Instance.Check(JudgeTask.SourceCode, JudgeTask.Language,
                out string unsafeCode, out int line))
            {
                result.ResultCode = JudgeResultCode.CompileError;
                result.JudgeDetail = "Include unsafe code, please remove them!";
                result.JudgeDetail += "\r\n";
                result.JudgeDetail += $"line {line}: {unsafeCode}";
                throw new ExpectedJudgeStopException("Unsafe code");
            }
        }

        protected void WriteSourceCode()
        {
            string sourceFileName = Path.Combine(Context.TempDirectory, LangConfig.SourceCodeFileName);
            File.WriteAllText(sourceFileName, JudgeTask.SourceCode);
        }

        protected void CompileSourceCode()
        {
            JudgeResult result = JudgeResult;
            if (!LangConfig.NeedCompile)
                return;

            Compiler compiler = new Compiler(Context);
            string compileResult = compiler.Compile();

            if (string.IsNullOrEmpty(compileResult))
                return;

            // compileResult不为空则编译有错误
            result.JudgeDetail = compileResult.Replace(Context.TempDirectory, "");
            result.ResultCode = JudgeResultCode.CompileError;
            result.MemoryCost = 0;
            throw new ExpectedJudgeStopException("Compile error");
        }

        protected ProgramTestDataFile[] GetTestDataFiles()
        {
            JudgeResult result = JudgeResult;
            ProgramTestDataFile[] dataFiles = TestDataManager.GetTestDataFiles(JudgeTask.ProblemId);
            if (dataFiles.Length > 0)
                return dataFiles;

            result.ResultCode = JudgeResultCode.JudgeFailed;
            result.JudgeDetail = "No test data.";
            throw new ExpectedJudgeStopException("No test data");
        }

        protected void JudgeAllCases(BaseSingleCaseJudger judger, ProgramTestDataFile[] dataFiles)
        {
            JudgeResult result = JudgeResult;
            int acceptedCasesCount = 0;
            foreach (var dataFile in dataFiles)
            {
                try
                {
                    ProgramTestData testData = TestDataManager.GetTestData(JudgeTask.ProblemId, dataFile);
                    SingleJudgeResult singleResult = judger.Judge(testData.Input, testData.Output);

                    if (result.ResultCode == JudgeResultCode.Accepted)
                    {
                        // 计算有时间补偿的总时间
                        result.TimeCost =
                            Math.Max(result.TimeCost, (int) (singleResult.TimeCost * LangConfig.TimeCompensation));
                        result.MemoryCost = Math.Max(result.MemoryCost, singleResult.MemoryCost);
                    }

                    if (singleResult.ResultCode == JudgeResultCode.Accepted)
                        acceptedCasesCount++;
                    else
                    {
                        // 出错时只记录第一组出错的信息
                        if (result.ResultCode == JudgeResultCode.Accepted)
                        {
                            result.ResultCode = singleResult.ResultCode;
                            result.JudgeDetail = singleResult.JudgeDetail;
                        }

                        if (!JudgeTask.JudgeAllCases)
                            break;
                    }
                }
                catch (Exception e)
                {
                    result.ResultCode = JudgeResultCode.JudgeFailed;
                    result.JudgeDetail = e.ToString();
                    throw new ExpectedJudgeStopException("Judge failed");
                }
            }

            result.JudgeDetail = result.JudgeDetail.Replace(Context.TempDirectory, "");
            result.PassRate = (double) acceptedCasesCount / dataFiles.Length;
        }

        public override void Dispose()
        {
            ProcessorAffinityManager.ReleaseUsage(JudgeTask.ProcessorAffinity);
            DeleteTempDirectory();
        }
    }
}