using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    public abstract class BaseTaskFetcher : ITaskFetcher
    {
        /// <summary>
        /// 配置信息
        /// </summary>
        protected Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// Http客户端
        /// </summary>
        protected HttpWebClient HttpClient { get; } = ConfiguredClient.Create();

        /// <summary>
        /// 从服务器取回JudgeTask
        /// </summary>
        /// <returns>JudgeContext数组</returns>
        public abstract JudgeContext[] Fetch();

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}