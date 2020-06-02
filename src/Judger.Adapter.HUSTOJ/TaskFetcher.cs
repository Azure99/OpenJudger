using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            List<JudgeContext> taskList = new List<JudgeContext>();
            string[] pendingSids = GetPending();
            foreach (string sid in pendingSids)
            {
                try
                {
                    taskList.Add(GetJudgeTask(sid));
                }
                catch (Exception ex)
                {
                    LogManager.Exception(ex);
                }
            }

            return taskList.ToArray();
        }

        private string[] GetPending()
        {
            Authenticator.Instance.CheckLogin();

            string response;
            try
            {
                string requestBody = CreateGetPendingRequestBody();
                response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody).Trim();
            }
            catch
            {
                return new string[0];
            }
            
            return response == "" ? new string[0] : Regex.Split(response, "\r\n|\r|\n");
        }

        private string CreateGetPendingRequestBody()
        {
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.Append("getpending=1&");

            bodyBuilder.Append("oj_lang_set=");
            foreach (ProgramLangConfig lang in Config.Languages)
                bodyBuilder.Append(lang.Name + ",");

            bodyBuilder.Remove(bodyBuilder.Length - 1, 1);
            bodyBuilder.Append("&");

            bodyBuilder.Append("max_running=" + Config.MaxQueueSize);
            return bodyBuilder.ToString();
        }

        private JudgeContext GetJudgeTask(string submitId)
        {
            GetSolutionInfo(submitId, out string problemId, out string author, out string lang);
            GetProblemInfo(problemId, out int timeLimit, out int memoryLimit, out bool spj);
            string sourceCode = GetSolution(submitId);
            string dateMd5 = GetTestDataMd5(problemId);

            JudgeContext task = JudgeContextFactory.Create(
                submitId, problemId, dateMd5, lang, sourceCode,
                author, timeLimit, memoryLimit, false, spj);

            Authenticator.Instance.UpdateSolution(submitId, 3, 0, 0, 0.0);

            return task;
        }

        private void GetSolutionInfo(string sid, out string problemId, out string username, out string lang)
        {
            string requestBody = "getsolutioninfo=1&sid=" + sid;
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            string[] split = Regex.Split(response, "\r\n|\r|\n");

            problemId = split[0];
            username = split[1];
            lang = split[2];
        }

        private string GetSolution(string sid)
        {
            string requestBody = $"getsolution=1&sid={sid}";

            return HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);
        }

        private void GetProblemInfo(string pid, out int timeLimit, out int memoryLimit, out bool spj)
        {
            string requestBody = $"getprobleminfo=1&pid={pid}";
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            string[] split = Regex.Split(response, "\r\n|\r|\n");

            timeLimit = (int) (double.Parse(split[0]) * 1000);
            memoryLimit = int.Parse(split[1]) * 1024;
            spj = int.Parse(split[2]) == 1;
        }

        private string GetTestDataMd5(string pid)
        {
            string requestBody = $"gettestdatalist=1&pid={pid}&time=1";
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            return Md5Encrypt.EncryptToHexString(response);
        }
    }
}