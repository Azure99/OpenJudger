using System;
using Judger.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher
{
    public abstract class BaseTaskSubmitter : ITaskSubmitter
    {
        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = ConfiguredClient.Create();

        /// <summary>
        /// 提交评测结果
        /// </summary>
        /// <param name="result">JudgeResult</param>
        /// <returns>是否成功</returns>
        public abstract bool Submit(JudgeResult result);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
