using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Judger.Entity;
using Judger.Fetcher.SDNUOJ.Entity;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// JudgeTask取回器
    /// </summary>
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            HttpClient.CookieContainer = Authenticator.Singleton.CookieContainer;
        }

        public override JudgeTask[] Fetch()
        {
            string resultString = HttpClient.UploadString(Config.TaskFetchUrl, CreateRequestBody(), 3);

            JudgeTask[] tasks = ParseTask(resultString);
            return tasks;
        }

        /// <summary>
        /// 创建请求Body
        /// </summary>
        private string CreateRequestBody()
        {
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.Append("count=1&");
            bodyBuilder.Append("supported_languages=");

            StringBuilder langBuilder = new StringBuilder();
            foreach (var lang in Config.Languages)
            {
                langBuilder.Append(lang.Language + "[],");
            }
            langBuilder.Remove(langBuilder.Length - 1, 1);

            bodyBuilder.Append(System.Web.HttpUtility.UrlEncode(langBuilder.ToString()));

            return bodyBuilder.ToString();
        }

        /// <summary>
        /// 从Response中解析JudgeTask
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        private JudgeTask[] ParseTask(string jsonStr)
        {
            JArray jArray = JArray.Parse(jsonStr);

            if (jArray.Count == 0)
            {
                return new JudgeTask[0];
            }

            JObject jObject = jArray[0] as JObject;
            if (!CheckTaskJObject(jObject))
            {
                return new JudgeTask[0];
            }

            SDNUOJTaskEntity taskEntity = jObject.ToObject<SDNUOJTaskEntity>();

            JudgeTask task = JudgeTaskFactory.Create(
                Int32.Parse(taskEntity.SubmitID), Int32.Parse(taskEntity.ProblemID), taskEntity.DataVersion,
                taskEntity.Language.Substring(0, taskEntity.Language.Length - 2), taskEntity.SourceCode, taskEntity.Author,
                Int32.Parse(taskEntity.TimeLimit), Int32.Parse(taskEntity.MemoryLimit), false);

            return new JudgeTask[1] { task };
        }

        /// <summary>
        /// 检查JsonObject是否符合JudgeTask规范
        /// </summary>
        private bool CheckTaskJObject(JObject obj)
        {
            HashSet<string> keySet = new HashSet<string>();
            foreach (JProperty key in obj.Properties())
            {
                keySet.Add(key.Name.ToLower());
            }

            if (!keySet.Contains("sid")         || !keySet.Contains("pid")      ||
                !keySet.Contains("dataversion") || !keySet.Contains("language") ||
                !keySet.Contains("sourcecode"))
            {
                return false;
            }

            return true;
        }
    }
}
