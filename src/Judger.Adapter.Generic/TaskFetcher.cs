using System.Linq;
using Judger.Adapter.Generic.Entity;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Utils;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic
{
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        public override JudgeContext[] Fetch()
        {
            string jsonResp = HttpClient.UploadString(Config.TaskFetchUrl, CreateRequestBody());
            ServerResponse response = Json.DeSerialize<ServerResponse>(jsonResp);

            return response.Code switch
            {
                ResponseCode.NoTask => new JudgeContext[0],
                ResponseCode.Success => CreateJudgeContexts(response.Data),
                _ => throw new AdapterException(response.Message)
            };
        }

        private string CreateRequestBody()
        {
            return TokenUtil.CreateJObject().ToString();
        }

        private JudgeContext[] CreateJudgeContexts(JToken jsonResp)
        {
            InnerJudgeTask[] tasks = jsonResp.ToObject<InnerJudgeTask[]>();

            if (tasks == null || tasks.Length == 0)
                return new JudgeContext[0];

            return tasks
                .Select(task => JudgeContextFactory.Create(
                    task.SubmitId, task.ProblemId, task.DataVersion, task.Language,
                    task.SourceCode, task.Author, task.TimeLimit, task.MemoryLimit,
                    task.JudgeAllCases, task.JudgeType))
                .ToArray();
        }
    }
}