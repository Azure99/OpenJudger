﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Judger.Core.Program.Entity;
using Judger.Entity;
using Judger.Managers;

namespace Judger.Core.Program
{
    public class SpecialJudger : BaseJudger
    {
        /// <summary>
        /// 注意:未编写:SPJTaks中的语言应对应SPJ程序，而不是JudgeTask
        /// </summary>
        private JudgeTask SPJTask;

        public SpecialJudger(JudgeTask task) : base(task)
        {
            JudgeTask.ProcessorAffinity = ProcessorAffinityManager.GetUseage();
            SPJTask = SPJManager.CreateSPJJudgeTask(task);
        }

        public override JudgeResult Judge()
        {
            //判题结果
            JudgeResult result = new JudgeResult
            {
                SubmitID = JudgeTask.SubmitID,
                ProblemID = JudgeTask.ProblemID,
                Author = JudgeTask.Author,
                JudgeDetail = "",
                MemoryCost = 0,
                TimeCost = 0,
                PassRate = 0,
                ResultCode = JudgeResultCode.Accepted
            };

            //正则恶意代码检查
            if (!CodeChecker.Singleton.CheckCode(JudgeTask.SourceCode, JudgeTask.Language, out string unsafeCode, out int line))
            {
                result.ResultCode = JudgeResultCode.CompileError;
                result.JudgeDetail = "Include unsafe code, please remove them!";
                result.JudgeDetail += "\r\n";
                result.JudgeDetail += "line " + line + ": " + unsafeCode;
                return result;
            }

            //构建SPJ程序
            BuildSpecialJudgeProgram();

            //写出源代码
            string sourceFileName = JudgeTask.TempJudgeDirectory + Path.DirectorySeparatorChar + JudgeTask.LangConfig.SourceCodeFileName;
            File.WriteAllText(sourceFileName, JudgeTask.SourceCode);

            //编译代码
            if (JudgeTask.LangConfig.NeedCompile)
            {
                Compiler compiler = new Compiler(JudgeTask);
                string compileRes = compiler.Compile();

                //检查是否有编译错误(compileRes不为空则代表有错误)
                if (!string.IsNullOrEmpty(compileRes))
                {
                    result.JudgeDetail = compileRes.Replace(JudgeTask.TempJudgeDirectory, "");//去除路径信息
                    result.ResultCode = JudgeResultCode.CompileError;
                    return result;
                }
            }

            //创建单例Judger
            SpecialSingleCaseJudger judger = new SpecialSingleCaseJudger(JudgeTask, SPJTask);

            //获取所有测试点文件名
            Tuple<string, string>[] dataFiles = TestDataManager.GetTestDataFilesName(JudgeTask.ProblemID);
            if (dataFiles.Length == 0)//无测试数据
            {
                result.ResultCode = JudgeResultCode.JudgeFailed;
                result.JudgeDetail = "No test data.";
                return result;
            }

            int acceptedCasesCount = 0;//通过的测试点数
            for (int i = 0; i < dataFiles.Length; i++)
            {
                try
                {
                    TestDataManager.GetTestData(JudgeTask.ProblemID, dataFiles[i].Item1, dataFiles[i].Item2, out string input, out string output);//读入测试数据

                    SingleJudgeResult singleRes = judger.Judge(input, output);//测试此测试点

                    //计算有时间补偿的总时间
                    result.TimeCost = Math.Max(result.TimeCost, (int)(singleRes.TimeCost * JudgeTask.LangConfig.TimeCompensation));
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
                        {
                            break;
                        }
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
            result.JudgeDetail = result.JudgeDetail.Replace(JudgeTask.TempJudgeDirectory, "");

            //通过率
            result.PassRate = (double)acceptedCasesCount / dataFiles.Length;

            return result;
        }

        /// <summary>
        /// 构建并写出供Judger使用的SPJ程序
        /// </summary>
        private void BuildSpecialJudgeProgram()
        {
            File.WriteAllText(
                Path.Combine(SPJTask.TempJudgeDirectory, SPJTask.LangConfig.SourceCodeFileName), 
                SPJTask.SourceCode);

            if (SPJTask.LangConfig.NeedCompile)
            {
                //构建SPJ程序
                if (TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemID) == null)
                {
                    if (!CompileSpecialJudgeProgram())
                    {
                        throw new CompileException("Can not build special judge program!");
                    }
                }

                SpecialJudgeProgram spjProgram = TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemID);
                File.WriteAllBytes(Path.Combine(SPJTask.TempJudgeDirectory, spjProgram.LangConfiguration.ProgramFileName), spjProgram.Program);
            }
        }

        /// <summary>
        /// 编译SPJ程序
        /// </summary>
        /// <returns>是否编译并写出成功</returns>
        private bool CompileSpecialJudgeProgram()
        {
            Compiler compiler = new Compiler(SPJTask);
            string compileResult = compiler.Compile();
            if (compileResult != "")
            {
                throw new CompileException("Can not compile special judge program!" + Environment.NewLine + compileResult);
            }

            string spjProgramPath =
                Path.Combine(SPJTask.TempJudgeDirectory, SPJTask.LangConfig.ProgramFileName);

            if (!File.Exists(spjProgramPath))
            {
                throw new CompileException("Special judge program not found!");
            }

            SpecialJudgeProgram spjProgram = new SpecialJudgeProgram
            {
                LangConfiguration = SPJTask.LangConfig,
                Program = File.ReadAllBytes(spjProgramPath)
            };

            TestDataManager.WriteSpecialJudgeProgramFile(JudgeTask.ProblemID, spjProgram);

            return TestDataManager.GetSpecialJudgeProgramFile(JudgeTask.ProblemID) != null;
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
                        Directory.Delete(JudgeTask.TempJudgeDirectory, true);//删除判题临时目录
                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        break;
                    }
                    catch
                    {
                        if (tryCount++ > 20)
                        {
                            throw new JudgeException("Cannot delete temp directory");
                        }
                        Thread.Sleep(500);
                    }
                }

            }, TaskCreationOptions.LongRunning).Start();
        }
    }
}