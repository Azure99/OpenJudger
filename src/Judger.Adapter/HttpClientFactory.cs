using Judger.Managers;
using Judger.Utils;

namespace Judger.Adapter
{
    /// <summary>
    /// Http客户端工厂
    /// </summary>
    /// 创建含有基本配置的HttpWebClient
    public static class HttpClientFactory
    {
        public static HttpWebClient Create()
        {
            return new HttpWebClient
            {
                Proxy = null,
                Timeout = ConfigManager.Config.FetchTimeout,
                ReadWriteTimeout = ConfigManager.Config.FetchTimeout,
                DefaultContentType = "application/x-www-form-urlencoded"
            };
        }
    }
}