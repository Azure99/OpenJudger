using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.HUSTOJ
{
    public class TaskFetcher : BaseTaskFetcher
    {
        public TaskFetcher()
        {
            Client.CookieContainer = Authenticator.Singleton.CookieContainer;
        }

        public override JudgeTask[] Fetch()
        {
            List<JudgeTask> taskList = new List<JudgeTask>();
            int[] pendingSid = GetPending();
            foreach(int sid in pendingSid)
            {
                try
                {
                    taskList.Add(GetJudgeTask(sid));
                }
                catch { }
            }

            return taskList.ToArray();
        }

        private int[] GetPending()
        {
            Authenticator.Singleton.CheckLogin();

            string res = "";
            try
            {
                string body = CreateGetPendingRequestBody();
                res = Client.UploadString(Config.TaskFetchUrl, body);
            }
            catch
            {
                return new int[0];
            }

            string[] split = Regex.Split(res, "\r\n|\r|\n");
            List<int> sidList = new List<int>();
            foreach (string s in split)
            {
                if (int.TryParse(s, out int sid))
                {
                    sidList.Add(sid);
                }
            }

            return sidList.ToArray();
        }

        private string CreateGetPendingRequestBody()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("getpending=1&");

            sb.Append("oj_lang_set=");
            foreach(var lang in Config.Languages)
            {
                sb.Append(lang.Language + ",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("&");

            sb.Append("max_running=" + Config.MaxQueueSize);
            return sb.ToString();
        }

        private JudgeTask GetJudgeTask(int submitID)
        {
            GetSolutionInfo(submitID, out int problemID, out string author, out string lang);
            string sourceCode = GetSolution(submitID);
            GetProblemInfo(problemID, out int timeLimit, out int memoryLimit, out bool spj);

            string dateMD5 = GetTestDataMD5(problemID);

            JudgeTask task = JudgeTaskFactory.Create(
                                submitID, problemID, dateMD5, lang, sourceCode, 
                                author, timeLimit, memoryLimit, spj);

            Authenticator.Singleton.UpdateSolution(submitID, 3, 0, 0, 0.0);

            return task;
        }

        /// <returns>ProblemID, Username, Language</returns>
        private void GetSolutionInfo(int sid, out int problemID, out string username, out string lang)
        {
            string body = "getsolutioninfo=1&sid=" + sid;
            string res = Client.UploadString(Config.TaskFetchUrl, body, 3);

            string[] split = Regex.Split(res, "\r\n|\r|\n");
            problemID = int.Parse(split[0]);
            username = split[1];
            lang = split[2];
        }

        private string GetSolution(int sid)
        {
            string body = "getsolution=1&sid=" + sid;
            string res = Client.UploadString(Config.TaskFetchUrl, body, 3);

            return res;
        }

        private void GetProblemInfo(int pid, out int timeLimit, out int memoryLimit, out bool spj)
        {
            string body = "getprobleminfo=1&pid=" + pid;
            string res = Client.UploadString(Config.TaskFetchUrl, body, 3);

            string[] split = Regex.Split(res, "\r\n|\r|\n");
            timeLimit = int.Parse(split[0]) * 1000;
            memoryLimit = int.Parse(split[1]) * 1024;
            spj = split[2] == "1";
        }

        private string GetTestDataMD5(int pid)
        {
            string body = string.Format("gettestdatalist=1&pid={0}&time=1", pid);
            string res = Client.UploadString(Config.TaskFetchUrl, body, 3);
            
            return MD5Encrypt.EncryptToHexString(res);
        }
    }
}
