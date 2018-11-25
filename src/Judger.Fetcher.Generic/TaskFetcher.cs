using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Judger.Models;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// JudgeTask取回器
    /// </summary>
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            Client.DefaultContentType = "application/json";
        }

        public override JudgeTask[] Fetch()
        {
            string resultString = Client.UploadString(Config.TaskFetchUrl, CreateRequestBody());

            return ParseTask(resultString);
        }

        //创建请求Body
        private string CreateRequestBody()
        {
            JObject requestObj = new JObject();
            requestObj.Add("JudgerName", Config.JudgerName);
            requestObj.Add("Token", Token.Create());

            return requestObj.ToString();
        }

        private JudgeTask[] ParseTask(string jsonStr)
        {
            JObject jsonObj = JObject.Parse(jsonStr);
            if(!CheckTaskJObject(jsonObj))
            {
                return new JudgeTask[0];
            }

            JudgeTask t = JObject.Parse(jsonStr).ToObject<JudgeTask>();

            JudgeTask task = JudgeTaskFactory.Create(
                t.SubmitID, t.ProblemID, t.DataVersion,
                t.Language, t.SourceCode, t.Author,
                t.TimeLimit, t.MemoryLimit, t.SpecialJudge);

            return new JudgeTask[1] { task };
        }

        /// <summary>
        /// 检查JsonObject是否符合JudgeTask规范
        /// </summary>
        private bool CheckTaskJObject(JObject obj)
        {
            HashSet<string> set = new HashSet<string>();
            foreach(JProperty key in obj.Properties())
            {
                set.Add(key.Name.ToLower());
            }

            if (!set.Contains("submitid")    || !set.Contains("problemid") ||
                !set.Contains("dataversion") || !set.Contains("language")  ||
                !set.Contains("sourcecode")) 
            {
                return false;
            }

            return true;
        }
    }
}
