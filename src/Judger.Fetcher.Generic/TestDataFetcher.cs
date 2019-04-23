using Newtonsoft.Json.Linq;
using Judger.Entity;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// TestData获取器
    /// </summary>
    public class TestDataFetcher : BaseTestDataFetcher
    {
        private const string JOBJECT_PROBLEM_ID = "problemId";
        
        /// <summary>
        /// TestData获取器
        /// </summary>
        public TestDataFetcher()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        /// <summary>
        /// 取回测试数据
        /// </summary>
        /// <param name="task">评测任务</param>
        /// <returns>测试数据</returns>
        public override byte[] Fetch(JudgeTask task)
        {
            return HttpClient.UploadData(Config.TestDataFetchUrl, CreateRequestBody(task.ProblemId), 3);
        }

        /// <summary>
        /// 创建取回测试数据的请求
        /// </summary>
        /// <param name="problemId">题目Id</param>
        /// <returns>取回测试数据的请求</returns>
        private string CreateRequestBody(int problemId)
        {
            JObject requestBody = Token.CreateJObject();
            requestBody.Add(JOBJECT_PROBLEM_ID, problemId);

            return requestBody.ToString();
        }
    }
}
