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
            string jsonResp = HttpClient.UploadString(Config.TaskFetchUrl, CreateRequestBody(), 3);
            return CreateJudgeContexts(jsonResp);
        }

        private string CreateRequestBody()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("count=1&");
            builder.Append("supported_languages=");

            StringBuilder langBuilder = new StringBuilder();

            foreach (ProgramLangConfig lang in Config.Languages)
                langBuilder.Append(lang.Name + "[],");
            foreach (DbLangConfig lang in Config.Databases)
                langBuilder.Append(lang.Name + "[],");

            langBuilder.Remove(langBuilder.Length - 1, 1);

            builder.Append(HttpUtility.UrlEncode(langBuilder.ToString()));
            return builder.ToString();
        }

        private JudgeContext[] CreateJudgeContexts(string jsonResp)
        {
            JArray taskJArray = JArray.Parse(jsonResp);

            if (taskJArray.Count == 0 || !(taskJArray[0] is JObject))
                return new JudgeContext[0];

            SdnuojJudgeTask task = taskJArray[0].ToObject<SdnuojJudgeTask>();
            if (task == null)
                return new JudgeContext[0];

            JudgeContext context = JudgeContextFactory.Create(
                task.SubmitId, task.ProblemId, task.DataVersion,
                task.Language.Substring(0, task.Language.Length - 2), task.SourceCode,
                task.Author, int.Parse(task.TimeLimit), int.Parse(task.MemoryLimit),
                false, false, bool.Parse(task.DbJudge));

            return new[] {context};
        }
    }
}