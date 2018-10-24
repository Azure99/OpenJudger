using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// TestData获取器
    /// </summary>
    public class TestDataFetcher : ITestDataFetcher
    {
        //最大测试数据下载时间
        private const int MAX_DOWNLOAD_TIME = 600000;
        private readonly Configuration _config = ConfigManager.Config;
        private HttpWebClient _httpClient = ConfiguredClient.Create();

        public TestDataFetcher()
        {
            _httpClient.ReadWriteTimeout = MAX_DOWNLOAD_TIME;
            _httpClient.DefaultContentType = "application/json";
        }

        public byte[] Fetch(string problemID)
        {
            string body = CreateRequestBody(problemID);

            return _httpClient.UploadData(_config.TestDataFetchUrl, body, 3);
        }

        //创建请求Body
        private string CreateRequestBody(string problemID)
        {
            JObject obj = new JObject();
            obj.Add("JudgerName", _config.JudgerName);
            obj.Add("Token", Token.Create());
            obj.Add("ProblemID", Int32.Parse(problemID));

            return obj.ToString();
        }

        public byte[] Fetch(int problemID)
        {
            return Fetch(problemID.ToString());
        }

        public byte[] Fetch(JudgeTask task)
        {
            return Fetch(task.ProblemID.ToString());
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
