using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    public abstract class BaseTaskSubmitter : ITaskSubmitter
    {
        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = HttpClientFactory.Create();

        public abstract void Submit(JudgeContext context);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}