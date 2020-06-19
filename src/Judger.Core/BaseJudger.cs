using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;

namespace Judger.Core
{
    /// <summary>
    /// Judger基类
    /// </summary>
    public abstract class BaseJudger : IDisposable
    {
        protected BaseJudger(JudgeContext context)
        {
            Context = context;
            JudgeTask = context.Task;
            JudgeResult = context.Result;
        }

        protected JudgeContext Context { get; }
        protected JudgeTask JudgeTask { get; }
        protected JudgeResult JudgeResult { get; }

        public virtual void Dispose()
        { }

        protected void DeleteTempDirectory()
        {
            // 判题结束时文件可能仍然被占用, 尝试删除
            new Task(() =>
            {
                int tryCount = 0;
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

        /// <summary>
        /// 评测此任务
        /// </summary>
        public abstract void Judge();
    }
}