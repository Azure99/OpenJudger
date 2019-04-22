using System;
using Newtonsoft.Json.Linq;
using Judger.Entity;

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
            return Fetch(task.ProblemId.ToString());
        }

        public byte[] Fetch(string problemId)
        {
            string requestBody = CreateRequestBody(problemId);

            return HttpClient.UploadData(Config.TestDataFetchUrl, requestBody, 3);
        }

        /// <summary>
        /// 创建请求Body
        /// </summary>
        /// <param name="problemId">问题ID</param>
        private string CreateRequestBody(string problemId)
        {
            JObject body = new JObject();
            body.Add("JudgerName", Config.JudgerName);
            body.Add("Token", Token.Create());
            body.Add("ProblemID", Int32.Parse(problemId));

            return body.ToString();
        }
    }
}
