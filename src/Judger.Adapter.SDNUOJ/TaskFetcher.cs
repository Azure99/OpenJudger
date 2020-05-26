using System.Collections.Generic;
using System.Text;
using System.Web;
using Judger.Adapter.SDNUOJ.Entity;
using Judger.Models;
using Judger.Models.Database;
using Judger.Models.Program;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.SDNUOJ
{
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

        private string CreateRequestBody()
        {
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.Append("count=1&");
            bodyBuilder.Append("supported_languages=");

            StringBuilder langBuilder = new StringBuilder();

            foreach (ProgramLangConfig lang in Config.Languages)
                langBuilder.Append(lang.Name + "[],");

            foreach (DbLangConfig lang in Config.Databases)
                langBuilder.Append(lang.Name + "[],");

            langBuilder.Remove(langBuilder.Length - 1, 1);

            bodyBuilder.Append(HttpUtility.UrlEncode(langBuilder.ToString()));
            return bodyBuilder.ToString();
        }

        private JudgeContext[] ParseTask(string jsonStr)
        {
            JArray jArray = JArray.Parse(jsonStr);

            if (jArray.Count == 0)
                return new JudgeContext[0];

            JObject jObject = jArray[0] as JObject;
            if (!CheckTaskJObject(jObject))
                return new JudgeContext[0];

            SdnuojTaskEntity entity = jObject.ToObject<SdnuojTaskEntity>();

            JudgeContext task = JudgeContextFactory.Create(
                entity.SubmitId, entity.ProblemId, entity.DataVersion,
                entity.Language.Substring(0, entity.Language.Length - 2), entity.SourceCode,
                entity.Author, int.Parse(entity.TimeLimit), int.Parse(entity.MemoryLimit),
                false, false, bool.Parse(entity.DbJudge));

            return new[] {task};
        }

        private bool CheckTaskJObject(JObject obj)
        {
            HashSet<string> keySet = new HashSet<string>();
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