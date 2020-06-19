using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Adapter.HUSTOJ
{
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
        }

        public override JudgeContext[] Fetch()
        {
            List<JudgeContext> contextList = new List<JudgeContext>();
            string[] pendingSids = GetPending();
            foreach (string sid in pendingSids)
            {
                try
                {
                    contextList.Add(CreateJudgeContext(sid));
                }
                catch (Exception ex)
                {
                    LogManager.Exception(ex);
                }
            }

            return contextList.ToArray();
        }

        private string[] GetPending()
        {
            string requestBody = CreateGetPendingRequestBody();
            string response = HttpPost(requestBody).Trim();

            return response == "" ? new string[0] : Regex.Split(response, "\r\n|\r|\n");
        }

        private string CreateGetPendingRequestBody()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("getpending=1&");

            builder.Append("oj_lang_set=");
            foreach (ProgramLangConfig lang in Config.Languages)
                builder.Append(lang.Name + ",");

            builder.Remove(builder.Length - 1, 1);
            builder.Append("&");

            builder.Append("max_running=" + Config.MaxQueueSize);
            return builder.ToString();
        }

        private JudgeContext CreateJudgeContext(string submitId)
        {
            GetSolutionInfo(submitId, out string problemId, out string author, out string lang);
            GetProblemInfo(problemId, out int timeLimit, out int memoryLimit, out bool spj);
            string sourceCode = GetSolutionSourceCode(submitId);
            string dataMd5 = GetTestDataMd5(problemId);

            JudgeContext context = JudgeContextFactory.Create(
                submitId, problemId, dataMd5, lang, sourceCode,
                author, timeLimit, memoryLimit, false, spj);

            UpdateSolution(submitId, 3, 0, 0, 0.0);

            return context;
        }

        private void GetSolutionInfo(string sid, out string problemId, out string username, out string language)
        {
            string requestBody = "getsolutioninfo=1&sid=" + sid;
            string response = HttpPost(requestBody);

            string[] infos = Regex.Split(response, "\r\n|\r|\n");

            problemId = infos[0];
            username = infos[1];
            language = infos[2];
        }

        private string GetSolutionSourceCode(string sid)
        {
            string requestBody = $"getsolution=1&sid={sid}";

            return HttpPost(requestBody);
        }

        private void GetProblemInfo(string pid, out int timeLimit, out int memoryLimit, out bool spj)
        {
            string requestBody = $"getprobleminfo=1&pid={pid}";
            string response = HttpPost(requestBody);

            string[] infos = Regex.Split(response, "\r\n|\r|\n");

            timeLimit = (int) (double.Parse(infos[0]) * 1000);
            memoryLimit = int.Parse(infos[1]) * 1024;
            spj = int.Parse(infos[2]) == 1;
        }

        private string GetTestDataMd5(string pid)
        {
            string requestBody = $"gettestdatalist=1&pid={pid}&time=1";
            string response = HttpPost(requestBody);

            return Md5Encrypt.EncryptToHexString(response);
        }

        private void UpdateSolution(string sid, int result, int timeCost, int memoryCost, double passRate)
        {
            string requestBody =
                $"update_solution=1&sid={sid}&result={result}&time={timeCost}" +
                $"&memory={memoryCost}&sim=0&simid=0&pass_rate={passRate}";

            HttpPost(requestBody);
        }

        private string HttpPost(string data)
        {
            return HttpClient.UploadString(Config.TaskFetchUrl, data, 3);
        }
    }
}