using System.Collections.Generic;
using System.Text;
using System.Web;
using Judger.Fetcher.SDNUOJ.Entity;
using Judger.Models;
using Judger.Models.Database;
using Judger.Models.Program;
using Newtonsoft.Json.Linq;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// JudgeTask取回器
    /// </summary>
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
        }

        public override JudgeContext[] Fetch()
        {
            string resultString = HttpClient.UploadString(Config.TaskFetchUrl, CreateRequestBody(), 3);

            JudgeContext[] tasks = ParseTask(resultString);
            return tasks;
        }

        /// <summary>
        /// 创建请求Body
        /// </summary>
        private string CreateRequestBody()
        {
            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append("count=1&");
            bodyBuilder.Append("supported_languages=");

            var langBuilder = new StringBuilder();
            foreach (ProgramLangConfig lang in Config.Languages)
                langBuilder.Append(lang.Name + "[],");

            foreach (DbLangConfig lang in Config.Databases)
                langBuilder.Append(lang.Name + "[],");

            langBuilder.Remove(langBuilder.Length - 1, 1);

            bodyBuilder.Append(HttpUtility.UrlEncode(langBuilder.ToString()));

            return bodyBuilder.ToString();
        }

        /// <summary>
        /// 从Response中解析JudgeTask
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        private JudgeContext[] ParseTask(string jsonStr)
        {
            JArray jArray = JArray.Parse(jsonStr);

            if (jArray.Count == 0)
                return new JudgeContext[0];

            var jObject = jArray[0] as JObject;
            if (!CheckTaskJObject(jObject))
                return new JudgeContext[0];

            var taskEntity = jObject.ToObject<SDNUOJTaskEntity>();

            JudgeContext task = JudgeContextFactory.Create(
                taskEntity.SubmitId, taskEntity.ProblemId, taskEntity.DataVersion,
                taskEntity.Language.Substring(0, taskEntity.Language.Length - 2), taskEntity.SourceCode,
                taskEntity.Author, int.Parse(taskEntity.TimeLimit), int.Parse(taskEntity.MemoryLimit),
                false, false, bool.Parse(taskEntity.DbJudge));

            return new[] {task};
        }

        /// <summary>
        /// 检查JsonObject是否符合JudgeTask规范
        /// </summary>
        private bool CheckTaskJObject(JObject obj)
        {
            var keySet = new HashSet<string>();
            foreach (JProperty key in obj.Properties())
                keySet.Add(key.Name.ToLower());

            return keySet.Contains("sid") &&
                   keySet.Contains("pid") &&
                   keySet.Contains("dataversion") &&
                   keySet.Contains("language") &&
                   keySet.Contains("sourcecode");
        }
    }
}