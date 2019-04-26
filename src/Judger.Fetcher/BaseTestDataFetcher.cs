using Judger.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher
{
    /// <summary>
    /// TestDataFetcher基类
    /// </summary>
    public abstract class BaseTestDataFetcher : ITestDataFetcher
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
        /// 最大测试数据下载时间
        /// </summary>
        private const int MAX_DOWNLOAD_TIME = 600000;
        
        /// <summary>
        /// TestDataFetcher基类
        /// </summary>
        protected BaseTestDataFetcher()
        {
            HttpClient.ReadWriteTimeout = MAX_DOWNLOAD_TIME;
        }

        /// <summary>
        /// 取回数据
        /// </summary>
        /// <param name="task">JudgeTask</param>
        /// <returns>byte[]形式的ZIP文件</returns>
        public abstract byte[] Fetch(JudgeTask task);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}