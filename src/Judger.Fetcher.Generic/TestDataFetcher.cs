using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Judger.Models;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// TestData获取器
    /// </summary>
    public class TestDataFetcher : BaseTestDataFetcher
    {
        public TestDataFetcher()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        public override byte[] Fetch(JudgeTask task)
        {
            return Fetch(task.ProblemID.ToString());
        }

        public byte[] Fetch(string problemID)
        {
            string requestBody = CreateRequestBody(problemID);

            return HttpClient.UploadData(Config.TestDataFetchUrl, requestBody, 3);
        }

        /// <summary>
        /// 创建请求Body
        /// </summary>
        /// <param name="problemID">问题ID</param>
        private string CreateRequestBody(string problemID)
        {
            JObject body = new JObject();
            body.Add("JudgerName", Config.JudgerName);
            body.Add("Token", Token.Create());
            body.Add("ProblemID", Int32.Parse(problemID));

            return body.ToString();
        }
    }
}
