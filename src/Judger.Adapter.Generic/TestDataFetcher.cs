using Judger.Models;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic
{
    public class TestDataFetcher : BaseTestDataFetcher
    {
        private const string FIELD_PROBLEM_ID = "problemId";

        public TestDataFetcher()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        public override byte[] Fetch(JudgeContext context)
        {
            string requestBody = CreateRequestBody(context.Task.ProblemId);
            return HttpClient.UploadData(Config.TestDataFetchUrl, requestBody, 3);
        }

        private string CreateRequestBody(string problemId)
        {
            JObject requestBody = TokenUtil.CreateJObject();
            requestBody.Add(FIELD_PROBLEM_ID, problemId);

            return requestBody.ToString();
        }
    }
}