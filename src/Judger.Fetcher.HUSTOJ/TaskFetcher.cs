using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Judger.Models;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Fetcher.HUSTOJ
{
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
        }

        public override JudgeContext[] Fetch()
        {
            var taskList = new List<JudgeContext>();
            string[] pendingSids = GetPending();
            foreach (string sid in pendingSids)
            {
                try
                {
                    taskList.Add(GetJudgeTask(sid));
                }
                catch
                { }
            }

            return taskList.ToArray();
        }

        /// <summary>
        /// 获取正在Pending的Solution ID
        /// </summary>
        private string[] GetPending()
        {
            Authenticator.Instance.CheckLogin();

            string response;
            try
            {
                string requestBody = CreateGetPendingRequestBody();
                response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody);
            }
            catch
            {
                return new string[0];
            }

            return Regex.Split(response, "\r\n|\r|\n");
        }

        /// <summary>
        /// 创建用于获取Pending提交的Http请求Body
        /// </summary>
        private string CreateGetPendingRequestBody()
        {
            var bodyBuilder = new StringBuilder();
            bodyBuilder.Append("getpending=1&");

            bodyBuilder.Append("oj_lang_set=");
            foreach (ProgramLangConfig lang in Config.Languages)
                bodyBuilder.Append(lang.Name + ",");

            bodyBuilder.Remove(bodyBuilder.Length - 1, 1);
            bodyBuilder.Append("&");

            bodyBuilder.Append("max_running=" + Config.MaxQueueSize);
            return bodyBuilder.ToString();
        }

        /// <summary>
        /// 根据SubmitID获取JudgeTask
        /// </summary>
        /// <param name="submitId">提交ID</param>
        private JudgeContext GetJudgeTask(string submitId)
        {
            GetSolutionInfo(submitId, out string problemId, out string author, out string lang);
            string sourceCode = GetSolution(submitId);
            GetProblemInfo(problemId, out int timeLimit, out int memoryLimit, out bool spj);
            string dateMd5 = GetTestDataMd5(problemId);

            JudgeContext task = JudgeContextFactory.Create(
                submitId, problemId, dateMd5, lang, sourceCode,
                author, timeLimit, memoryLimit, false, spj);

            Authenticator.Instance.UpdateSolution(submitId, 3, 0, 0, 0.0);

            return task;
        }

        /// <summary>
        /// 根据提交ID获取提交信息
        /// </summary>
        /// <param name="sid">提交ID</param>
        /// <param name="problemId">问题ID</param>
        /// <param name="username">用户名</param>
        /// <param name="lang">编程语言</param>
        private void GetSolutionInfo(string sid, out string problemId, out string username, out string lang)
        {
            string requestBody = "getsolutioninfo=1&sid=" + sid;
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            string[] split = Regex.Split(response, "\r\n|\r|\n");

            problemId = split[0];
            username = split[1];
            lang = split[2];
        }

        /// <summary>
        /// 根据提交ID获取源代码
        /// </summary>
        /// <param name="sid">提交ID</param>
        /// <returns>源代码</returns>
        private string GetSolution(string sid)
        {
            string requestBody = "getsolution=1&sid=" + sid;

            return HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);
        }

        /// <summary>
        /// 根据问题ID获取问题信息
        /// </summary>
        /// <param name="pid">问题信息</param>
        /// <param name="timeLimit">时间限制</param>
        /// <param name="memoryLimit">内存限制</param>
        /// <param name="spj">是否为Special Judge</param>
        private void GetProblemInfo(string pid, out int timeLimit, out int memoryLimit, out bool spj)
        {
            string requestBody = "getprobleminfo=1&pid=" + pid;
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            string[] split = Regex.Split(response, "\r\n|\r|\n");

            timeLimit = int.Parse(split[0]) * 1000;
            memoryLimit = int.Parse(split[1]) * 1024;
            spj = int.Parse(split[2]) == 1;
        }

        /// <summary>
        /// 根据问题ID获取测试数据的MD5
        /// </summary>
        /// <param name="pid">问题ID</param>
        private string GetTestDataMd5(string pid)
        {
            string requestBody = $"gettestdatalist=1&pid={pid}&time=1";
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            return Md5Encrypt.EncryptToHexString(response);
        }
    }
}