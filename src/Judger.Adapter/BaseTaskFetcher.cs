using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    /// <summary>
    /// TaskFetcher基类
    /// </summary>
    /// 用于从后端取回评测任务
    public abstract class BaseTaskFetcher : ITaskFetcher
    {
        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = WebClientFactory.Create();

        /// <summary>
        /// 从后端取回任务并创建评测上下文
        /// 可返回多个任务的上下文, 如果没有任务, 返回空数组
        /// </summary>
        public abstract JudgeContext[] Fetch();

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}