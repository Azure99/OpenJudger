using System;
using System.Collections.Generic;
using System.Text;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher
{
    public abstract class BaseTaskFetcher : ITaskFetcher
    {
        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient Client { get; } = ConfiguredClient.Create();

        /// <summary>
        /// 从服务器取回JudgeTask
        /// </summary>
        /// <returns>JudgeTask数组</returns>
        public abstract JudgeTask[] Fetch();

        public virtual void Dispose()
        {
            Client.Dispose();
        }
    }
}
