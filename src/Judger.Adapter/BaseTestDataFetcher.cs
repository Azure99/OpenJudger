using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    public abstract class BaseTestDataFetcher : ITestDataFetcher
    {
        private const int ConstMaxDownloadTime = 600000;

        protected BaseTestDataFetcher()
        {
            HttpClient.ReadWriteTimeout = ConstMaxDownloadTime;
        }

        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = HttpClientFactory.Create();

        public abstract byte[] Fetch(JudgeContext context);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}