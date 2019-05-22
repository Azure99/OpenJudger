using System.Collections.Generic;
using Judger.Fetcher.Generic.Entity;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// JudgeTask取回器
    /// </summary>
    public class TaskFetcher : BaseTaskFetcher
    {
        /// <summary>
        /// JudgeTask取回器
        /// </summary>
        public TaskFetcher()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        /// <summary>
        /// 尝试取回评测任务
        /// </summary>
        /// <returns>评测任务</returns>
        public override JudgeTask[] Fetch()
        {
            string response = HttpClient.UploadString(Config.TaskFetchUrl, CreateRequestBody());

            return ParseTask(response);
        }

        /// <summary>
        /// 创建取回评测任务的请求
        /// </summary>
        /// <returns>取回评测任务的请求</returns>
        private string CreateRequestBody()
        {
            return Token.CreateJObject().ToString();
        }

        /// <summary>
        /// 从Response中解析Task
        /// </summary>
        /// <returns>JudgeTasks</returns>
        private JudgeTask[] ParseTask(string jsonString)
        {
            InnerJudgeTask[] innerJudgeTasks = SampleJsonSerializer.DeSerialize<InnerJudgeTask[]>(jsonString);

            if (innerJudgeTasks == null || innerJudgeTasks.Length == 0)
                return new JudgeTask[0];

            List<JudgeTask> judgeTasks = new List<JudgeTask>();
            foreach (var item in innerJudgeTasks)
            {
                JudgeTask task = JudgeTaskFactory.Create(
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