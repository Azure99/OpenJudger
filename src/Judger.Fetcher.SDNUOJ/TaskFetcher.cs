using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Judger.Models;
using Judger.Fetcher.SDNUOJ.Models;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// JudgeTask取回器
    /// </summary>
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            Client.CookieContainer = Authenticator.Singleton.CookieContainer;
        }

        public override JudgeTask[] Fetch()
        {
            string resultString = Client.UploadString(Config.TaskFetchUrl, CreateRequestBody(), 3);

            JudgeTask[] tasks = ParseTask(resultString);
            return tasks;
        }

        //创建请求Body
        private string CreateRequestBody()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("count=1&");
            sb.Append("supported_languages=");

            StringBuilder langSB = new StringBuilder();
            foreach (var lang in Config.Languages)
            {
                langSB.Append(lang.Language + "[],");
            }
            langSB.Remove(langSB.Length - 1, 1);

            sb.Append(System.Web.HttpUtility.UrlEncode(langSB.ToString()));

            return sb.ToString();
        }

        private JudgeTask[] ParseTask(string jsonStr)
        {
            JArray arr = JArray.Parse(jsonStr);

            if (arr.Count == 0)
            {
                return new JudgeTask[0];
            }

            JObject obj = arr[0] as JObject;
            if (!CheckTaskJObject(obj))
            {
                return new JudgeTask[0];
            }

            SDNUOJJudgeTask t = obj.ToObject<SDNUOJJudgeTask>();

            JudgeTask task = JudgeTaskFactory.Create(
                Int32.Parse(t.SubmitID), Int32.Parse(t.ProblemID), t.DataVersion,
                t.Language.Substring(0, t.Language.Length - 2), t.SourceCode, t.Author,
                Int32.Parse(t.TimeLimit), Int32.Parse(t.MemoryLimit), false);

            return new JudgeTask[1] { task };
        }

        /// <summary>
        /// 检查JsonObject是否符合JudgeTask规范
        /// </summary>
        private bool CheckTaskJObject(JObject obj)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (JProperty key in obj.Properties())
            {
                set.Add(key.Name.ToLower());
            }

            if (!set.Contains("sid") || !set.Contains("pid") ||
                !set.Contains("dataversion") || !set.Contains("language") ||
                !set.Contains("sourcecode"))
            {
                return false;
            }

            return true;
        }
    }
}
