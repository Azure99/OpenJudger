﻿using System;
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
    public class ProgramJudger : BaseJudger
    {
        public ProgramJudger(JudgeContext context) : base(context)
        {
            JudgeTask.ProcessorAffinity = ProcessorAffinityManager.GetUsage();
            LangConfig = context.LangConfig as ProgramLangConfig;
        }

        private ProgramLangConfig LangConfig { get; }

        public override void Judge()
        {
            JudgeResult result = Context.Result;

            if (!CodeChecker.Instance.CheckCode(JudgeTask.SourceCode, JudgeTask.Language, out string unsafeCode,
                out int line))
            {
                result.ResultCode = JudgeResultCode.CompileError;
                result.JudgeDetail = "Include unsafe code, please remove them!";
                result.JudgeDetail += "\r\n";
                result.JudgeDetail += "line " + line + ": " + unsafeCode;
                return;
            }

            // 写出源代码
            string sourceFileName = Path.Combine(Context.TempDirectory, LangConfig.SourceCodeFileName);
            File.WriteAllText(sourceFileName, JudgeTask.SourceCode);

            // 编译代码
            if (LangConfig.NeedCompile)
            {
                var compiler = new Compiler(Context);
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
            var judger = new SingleCaseJudger(Context);

            //获取所有测试点文件名
            Tuple<string, string>[] dataFiles = TestDataManager.GetTestDataFilesName(JudgeTask.ProblemId);
            if (dataFiles.Length == 0) //无测试数据
            {
                result.ResultCode = JudgeResultCode.JudgeFailed;
                result.JudgeDetail = "No test data.";
                return;
            }

            var acceptedCasesCount = 0; //通过的测试点数
            for (var i = 0; i < dataFiles.Length; i++)
            {
                try
                {
                    //读入测试数据
                    TestDataManager.GetTestData(
                        JudgeTask.ProblemId, dataFiles[i].Item1, dataFiles[i].Item2,
                        out string input, out string output);

                    SingleJudgeResult singleRes = judger.Judge(input, output); //测试此测试点

                    // 评测所有测试点时, 只记录第一组出错的信息
                    if (result.ResultCode == JudgeResultCode.Accepted)
                    {
                        // 计算有时间补偿的总时间
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
                        // 出错时记录第一组出错的信息
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

            //去除目录信息
            result.JudgeDetail = result.JudgeDetail.Replace(Context.TempDirectory, "");

            //通过率
            result.PassRate = (double) acceptedCasesCount / dataFiles.Length;
        }

        public override void Dispose()
        {
            // 释放占用的独立处理器核心
            ProcessorAffinityManager.ReleaseUsage(JudgeTask.ProcessorAffinity);
            DeleteTempDirectory();
        }

        /// <summary>
        /// 删除临时目录
        /// </summary>
        private void DeleteTempDirectory()
        {
            //判题结束时文件可能仍然被占用，尝试删除
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