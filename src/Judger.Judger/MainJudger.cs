using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Judger.Managers;
using Judger.Models;
using Judger.Judger.Compilers;
using Judger.Judger.Models;

namespace Judger.Judger
{
    /// <summary>
    /// JudgeTask Judger
    /// </summary>
    public class MainJudger : IDisposable
    {
        /// <summary>
        /// 分配的JugdgeTask
        /// </summary>
        public JudgeTask JudgeTask { get; }

        public MainJudger(JudgeTask task)
        {
            JudgeTask = task;
            // 分配独立的处理器核心
            task.ProcessorAffinityUseage = ProcessorAffinityManager.GetUseage();
        }

        /// <summary>
        /// 评测此任务
        /// </summary>
        /// <returns>评测结果</returns>
        public JudgeResult Judge()
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

            //创建临时目录
            if (!Directory.Exists(JudgeTask.TempJudgeDirectory))
            {
                Directory.CreateDirectory(JudgeTask.TempJudgeDirectory);
            }

            //写出源代码
            string sourceFileName = JudgeTask.TempJudgeDirectory + Path.DirectorySeparatorChar + JudgeTask.LangConfig.SourceCodeFileName;
            File.WriteAllText(sourceFileName, JudgeTask.SourceCode);

            //编译代码
            if(JudgeTask.LangConfig.NeedCompile)
            {
                ICompiler compiler = CompilerFactory.Create(JudgeTask);
                //使用分配的独立处理器核心
                compiler.ProcessorAffinity = JudgeTask.ProcessorAffinityUseage;

                string compileRes = compiler.Compile(JudgeTask.LangConfig.CompilerArgs);

                //检查是否有编译错误(compileRes不为空则代表有错误)
                if (!string.IsNullOrEmpty(compileRes))
                {
                    result.JudgeDetail = compileRes.Replace(JudgeTask.TempJudgeDirectory, "");//去除路径信息
                    result.ResultCode = JudgeResultCode.CompileError;
                    return result;
                }
            }


            //创建单例Judger
            ISingleJudger judger = SingleJudgerFactory.Create(JudgeTask);
            judger.ProcessorAffinity = JudgeTask.ProcessorAffinityUseage;

            //获取所有测试点文件名
            Tuple<string, string>[] dataFiles = TestDataManager.GetTestDataFilesName(JudgeTask.ProblemID);
            if(dataFiles.Length == 0)//无测试数据
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
                        break;
                    }
                }
                catch(Exception e)
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

        public void Dispose()
        {
            // 释放占用的独立处理器核心
            ProcessorAffinityManager.ReleaseUseage(JudgeTask.ProcessorAffinityUseage);
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
