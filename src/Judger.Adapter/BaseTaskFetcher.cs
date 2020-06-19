using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    public abstract class BaseTaskFetcher : ITaskFetcher
    {
        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = HttpClientFactory.Create();

        public abstract JudgeContext[] Fetch();

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}