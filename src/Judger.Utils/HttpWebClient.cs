using System;
using System.Net;
using System.Text;

namespace Judger.Utils
{
    /// <summary>
    /// 扩展的 WebClient, 支持自动重试, 限制读写时间, 定义ContentType, Cookie
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
        /// 将指定的字符串上载到指定的资源使用 POST 方法
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="data">Post发送的数据</param>
        /// <param name="maxTry">最大尝试次数</param>
        /// <returns>请求结果</returns>
        public string UploadString(string address, string data, int maxTry)
        {
            var tryCount = 0;
            var lastEx = new Exception();

            while (tryCount++ < maxTry)
            {
                try
                {
                    return UploadString(address, data);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }
            }

            throw lastEx;
        }

        /// <summary>
        /// 下载请求的资源
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="maxTry">最大尝试次数</param>
        /// <returns>请求结果</returns>
        public string DownloadString(string address, int maxTry)
        {
            var tryCount = 0;
            var lastEx = new Exception();

            while (tryCount++ < maxTry)
            {
                try
                {
                    return DownloadString(address);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }
            }

            throw lastEx;
        }

        /// <summary>
        /// 尝试将指定的字符串上载到指定的资源使用 POST 方法
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="data">Post发送的数据</param>
        /// <param name="result">响应结果</param>
        /// <returns>是否成功</returns>
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
        /// <param name="address">地址</param>
        /// <param name="data">Post发送的数据</param>
        /// <param name="maxTry">最大尝试次数</param>
        /// <param name="result">响应结果</param>
        /// <returns>是否成功</returns>
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
        /// <param name="address">地址</param>
        /// <param name="result">请求结果</param>
        /// <returns>是否成功</returns>
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
        /// <param name="address">地址</param>
        /// <param name="result">请求结果</param>
        /// <param name="maxTray">最大尝试次数</param>
        /// <returns>是否成功</returns>
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
        /// <param name="address">地址</param>
        /// <param name="data">Post发送的数据</param>
        /// <returns>请求结果</returns>
        public byte[] UploadData(string address, string data)
        {
            return UploadData(address, Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// POST请求数据
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="data">Post发送的数据</param>
        /// <param name="maxTry">最大尝试次数</param>
        /// <returns>请求结果</returns>
        public byte[] UploadData(string address, string data, int maxTry)
        {
            var tryCount = 0;
            var lastEx = new Exception();

            while (tryCount++ < maxTry)
            {
                try
                {
                    return UploadData(address, data);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }
            }

            throw lastEx;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest) base.GetWebRequest(address);

            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;
            request.CookieContainer = CookieContainer;

            // 请求无ContentType
            if (string.IsNullOrEmpty(request.ContentType))
                // 设置了默认ContentType
                if (!string.IsNullOrEmpty(DefaultContentType))
                    request.ContentType = DefaultContentType;

            return request;
        }
    }
}