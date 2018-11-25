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
            Client.DefaultContentType = "application/json";
        }

        public override byte[] Fetch(JudgeTask task)
        {
            return Fetch(task.ProblemID.ToString());
        }

        public byte[] Fetch(string problemID)
        {
            string body = CreateRequestBody(problemID);

            return Client.UploadData(Config.TestDataFetchUrl, body, 3);
        }

        //创建请求Body
        private string CreateRequestBody(string problemID)
        {
            JObject obj = new JObject();
            obj.Add("JudgerName", Config.JudgerName);
            obj.Add("Token", Token.Create());
            obj.Add("ProblemID", Int32.Parse(problemID));

            return obj.ToString();
        }
    }
}
