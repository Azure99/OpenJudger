using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    /// <summary>
    /// TestDataFetcher基类
    /// </summary>
    /// 用于从后端拉取测试数据
    public abstract class BaseTestDataFetcher : ITestDataFetcher
    {
        private const int MAX_DOWNLOAD_TIME = 600000;

        protected BaseTestDataFetcher()
        {
            HttpClient.ReadWriteTimeout = MAX_DOWNLOAD_TIME;
        }

        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = WebClientFactory.Create();

        /// <summary>
        /// 从后端拉取题目的测试数据
        /// </summary>
        /// <param name="context">评测上下文</param>
        /// <returns>byte[]形式的Zip文件</returns>
        public abstract byte[] Fetch(JudgeContext context);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}