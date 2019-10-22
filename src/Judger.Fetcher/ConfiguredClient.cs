﻿using Judger.Managers;
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
            var client = new HttpWebClient
            {
                Proxy = null,
                Timeout = ConfigManager.Config.FetchTimeout,
                ReadWriteTimeout = ConfigManager.Config.FetchTimeout,
                DefaultContentType = "application/x-www-form-urlencoded"
            };

            return client;
        }
    }
}