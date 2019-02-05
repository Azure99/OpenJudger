using System;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher
{
    public static class ConfiguredClient
    {
        /// <summary>
        /// 创建含有基本配置的HttpWebClient
        /// </summary>
        /// <returns></returns>
        public static HttpWebClient Create()
        {
            HttpWebClient client = new HttpWebClient();
            client.Proxy = null;
            client.Timeout = ConfigManager.Config.FetchTimeout;
            client.ReadWriteTimeout = ConfigManager.Config.FetchTimeout;
            client.DefaultContentType = "application/x-www-form-urlencoded";

            return client;
        }
    }
}
