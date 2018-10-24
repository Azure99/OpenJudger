using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Judger.Utils
{
    /// <summary>
    /// 扩展的 WebClient
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

        public HttpWebClient() : base()
        { }

        /// <summary>
        /// 将指定的字符串上载到指定的资源使用 POST 方法
        /// </summary>
        /// <param name="address">地址</param>
        /// <param name="data">Post发送的数据</param>
        /// <param name="maxTry">最大尝试次数</param>
        /// <returns>请求结果</returns>
        public string UploadString(string address, string data, int maxTry)
        {
            int tryCount = 0;
            Exception lastEx = new Exception();
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
            int tryCount = 0;
            Exception lastEx = new Exception();
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
            int tryCount = 0;
            Exception lastEx = new Exception();
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
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = Timeout;
            request.ReadWriteTimeout = ReadWriteTimeout;
            request.CookieContainer = CookieContainer;

            if (string.IsNullOrEmpty(request.ContentType)) //如果请求无ContentType
            {
                if (!string.IsNullOrEmpty(DefaultContentType)) //如果设置了DefaultContentType
                {
                    request.ContentType = DefaultContentType;
                }
            }

            return request;
        }
    }
}
