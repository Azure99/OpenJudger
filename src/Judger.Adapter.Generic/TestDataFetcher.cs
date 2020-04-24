﻿using Judger.Models;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic
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
        /// <param name="context">评测任务</param>
        /// <returns>测试数据</returns>
        public override byte[] Fetch(JudgeContext context)
        {
            string url = Config.TestDataFetchUrl;
            string requestBody = CreateRequestBody(context.Task.ProblemId);
            return HttpClient.UploadData(url, requestBody, 3);
        }

        /// <summary>
        /// 创建取回测试数据的请求
        /// </summary>
        /// <param name="problemId">题目Id</param>
        /// <returns>取回测试数据的请求</returns>
        private string CreateRequestBody(string problemId)
        {
            JObject requestBody = Token.CreateJObject();
            requestBody.Add(JOBJECT_PROBLEM_ID, problemId);

            return requestBody.ToString();
        }
    }
}