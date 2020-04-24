using System.Collections.Generic;
using Judger.Adapter.Generic.Entity;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Utils;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic
{
    /// <summary>
    /// JudgeTask取回器
    /// </summary>
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        /// <summary>
        /// 尝试取回评测任务
        /// 此方法由JudgeService定期调用以实现任务轮询
        /// 如果没有评测任务，返回空JudgeContext数组
        /// </summary>
        /// <returns>评测任务</returns>
        public override JudgeContext[] Fetch()
        {
            string url = Config.TaskFetchUrl;
            string requestBody = CreateRequestBody();

            string responseData = HttpClient.UploadString(url, requestBody);
            var response = Json.DeSerialize<ServerResponse>(responseData);

            if (response.Code == ResponseCode.NoTask)
                return new JudgeContext[0];

            if (response.Code == ResponseCode.Fail || response.Code == ResponseCode.WrongToken)
                throw new AdapterException(response.Message);

            return CreateTaskContexts(response.Data);
        }

        private string CreateRequestBody()
        {
            return Token.CreateJObject().ToString();
        }

        /// <summary>
        /// 从Response中解析Task
        /// </summary>
        /// <param name="data">来自服务器的JObject任务数组</param>
        /// <returns>JudgeContexts</returns>
        private JudgeContext[] CreateTaskContexts(JToken data)
        {
            var innerJudgeTasks = data.ToObject<InnerJudgeTask[]>();

            if (innerJudgeTasks == null || innerJudgeTasks.Length == 0)
                return new JudgeContext[0];

            var judgeTasks = new List<JudgeContext>();
            foreach (InnerJudgeTask item in innerJudgeTasks)
            {
                JudgeContext task = JudgeContextFactory.Create(
                    item.SubmitId, item.ProblemId, item.DataVersion,
                    item.Language, item.SourceCode, item.Author,
                    item.TimeLimit, item.MemoryLimit,
                    item.JudgeAllCases, item.JudgeType);

                judgeTasks.Add(task);
            }

            return judgeTasks.ToArray();
        }
    }
}