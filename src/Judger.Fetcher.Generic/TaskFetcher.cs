using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Judger.Entity;

namespace Judger.Fetcher.Generic
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

        public override JudgeTask[] Fetch()
        {
            string response = HttpClient.UploadString(Config.TaskFetchUrl, CreateRequestBody());

            return ParseTask(response);
        }

        /// <summary>
        /// 创建请求Body
        /// </summary>
        private string CreateRequestBody()
        {
            JObject requestBody = new JObject();
            requestBody.Add("JudgerName", Config.JudgerName);
            requestBody.Add("Token", Token.Create());

            return requestBody.ToString();
        }

        /// <summary>
        /// 从ResponseBody中解析Task
        /// </summary>
        private JudgeTask[] ParseTask(string jsonString)
        {
            JObject jObject = JObject.Parse(jsonString);
            if(!CheckTaskJObject(jObject))
            {
                return new JudgeTask[0];
            }

            JudgeTask tempTask = JObject.Parse(jsonString).ToObject<JudgeTask>();

            JudgeTask task = JudgeTaskFactory.Create(
                tempTask.SubmitId, tempTask.ProblemId, tempTask.DataVersion,
                tempTask.Language, tempTask.SourceCode, tempTask.Author,
                tempTask.TimeLimit, tempTask.MemoryLimit, tempTask.JudgeAllCases, tempTask.JudgeType);

            return new JudgeTask[] { task };
        }

        /// <summary>
        /// 检查JsonObject是否符合JudgeTask规范
        /// </summary>
        private bool CheckTaskJObject(JObject obj)
        {
            HashSet<string> keySet = new HashSet<string>();
            foreach(JProperty key in obj.Properties())
            {
                keySet.Add(key.Name.ToLower());
            }

            if (!keySet.Contains("submitid")    || !keySet.Contains("problemid") ||
                !keySet.Contains("dataversion") || !keySet.Contains("language")  ||
                !keySet.Contains("sourcecode")) 
            {
                return false;
            }

            return true;
        }
    }
}
