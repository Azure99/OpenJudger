using Judger.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher
{
    public abstract class BaseTaskFetcher : ITaskFetcher
    {
        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = ConfiguredClient.Create();

        /// <summary>
        /// 从服务器取回JudgeTask
        /// </summary>
        /// <returns>JudgeTask数组</returns>
        public abstract JudgeTask[] Fetch();

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}
