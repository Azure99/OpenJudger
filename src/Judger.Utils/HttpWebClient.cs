using System;
using System.Net;
using System.Text;

namespace Judger.Utils
{
    /// <summary>
    /// 扩展的WebClient
    /// 支持自动重试, 限制读写时间, 定义ContentType, Cookie等操作
    /// </summary>
    public class HttpWebClient : WebClient
    {
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; } = 100000;

        /// <summary>
        /// 读写超时时间
        /// </summary>
        public int ReadWriteTimeout { get; set; } = 300000;

        /// <summary>
        /// 默认ContentType
        /// </summary>
        public string DefaultContentType { get; set; } = "";

        /// <summary>
        /// Cookie容器
        /// </summary>
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        /// <summary>
        /// 将字符串上传到指定的位置, 使用POST方法
        /// </summary>
        public string UploadString(string address, string data, int maxTry)
        {
            int tryCount = 0;
            Exception lastException = new Exception();

            while (tryCount++ < maxTry)
            {
                try
                {
                    return UploadString(address, data);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw lastException;
        }

        /// <summary>
        /// 下载请求的资源
        /// </summary>
        public string DownloadString(string address, int maxTry)
        {
            int tryCount = 0;
            Exception lastException = new Exception();

            while (tryCount++ < maxTry)
            {
                try
                {
                    return DownloadString(address);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw lastException;
        }

        /// <summary>
        /// 尝试将字符串上传到指定的位置, 使用POST方法
        /// </summary>
        public bool TryUploadString(string address, string data, out string result)
        {
            try
            {
                result = UploadString(address, data);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// 尝试POST请求String
        /// </summary>
        public bool TryUploadString(string address, string data, int maxTry, out string result)
        {
            try
            {
                result = UploadString(address, data, maxTry);
                return true;
            }
            catch
            {
                result = "";
                return false;
            }
        }

        /// <summary>
        /// 尝试下载String
        /// </summary>
        public bool TryDownloadString(string address, out string result)
        {
            try
            {
                result = DownloadString(address);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// 尝试下载String
        /// </summary>
        public bool TryDownloadString(string address, out string result, int maxTray)
        {
            try
            {
                result = DownloadString(address);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// POST请求数据
        /// </summary>
        public byte[] UploadData(string address, string data)
        {
            return UploadData(address, Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// POST请求数据
        /// </summary>
        public byte[] UploadData(string address, string data, int maxTry)
        {
            int tryCount = 0;
            Exception lastException = new Exception();

            while (tryCount++ < maxTry)
            {
                try
                {
                    return UploadData(address, data);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw lastException;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest) base.GetWebRequest(address);
            if (request == null)
                throw new WebException("WebRequest not found");

            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;
            request.CookieContainer = CookieContainer;

            // 请求无ContentType
            if (string.IsNullOrEmpty(request.ContentType))
            {
                // 设置了默认ContentType
                if (!string.IsNullOrEmpty(DefaultContentType))
                    request.ContentType = DefaultContentType;
            }

            return request;
        }
    }
}