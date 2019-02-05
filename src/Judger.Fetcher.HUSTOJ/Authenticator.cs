using System;
using System.Net;
using Judger.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher.HUSTOJ
{
    public class Authenticator
    {
        public static Authenticator Singleton { get; set; }

        public CookieContainer CookieContainer { get; private set; }
        private HttpWebClient _httpClient = ConfiguredClient.Create();

        private Configuration _config = ConfigManager.Config;
        private string loginUrl = "http://localhost/login.php";

        private int delayCheckCount = 0;
        static Authenticator()
        {
            Singleton = new Authenticator();
        }

        private Authenticator()
        {
            CookieContainer = new CookieContainer();
            _httpClient.CookieContainer = CookieContainer;

            if (_config.AdditionalConfig.ContainsKey("LoginUrl"))
            {
                loginUrl = _config.AdditionalConfig["LoginUrl"];
            }
            else
            {
                _config.AdditionalConfig["LoginUrl"] = loginUrl;
                ConfigManager.SaveConfig();
            }

            Login();
        }

        /// <summary>
        /// 登录HUSTOJ
        /// </summary>
        public bool Login()
        {
            if (CheckLogin())
            {
                return true;
            }

            string requestBody = string.Format("user_id={0}&password={1}", _config.JudgerName, _config.Password);
            try
            {
                string response = _httpClient.UploadString(loginUrl, requestBody, 3);
            }
            catch { }

            return CheckLogin();
        }

        /// <summary>
        /// 检查是否已登录
        /// </summary>
        public bool CheckLogin()
        {
            string requestBody = "checklogin=1";
            try
            {
                string response = _httpClient.UploadString(_config.TaskFetchUrl, requestBody);
                return response == "1";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 定期检查并登录
        /// </summary>
        public void CheckAndLogin()
        {
            if(delayCheckCount++ > 10)
            {
                delayCheckCount = 0;
                Login();
            }
        }

        /// <summary>
        /// 更新判题状态
        /// </summary>
        /// <param name="sid">Solution ID</param>
        /// <param name="result">结果</param>
        /// <param name="time">时间</param>
        /// <param name="memory">内存消耗</param>
        /// <param name="passRate">通过率</param>
        public void UpdateSolution(int sid, int result, int time, int memory, double passRate)
        {
            string requestBody = 
                string.Format(
                    "update_solution=1&sid={0}&result={1}&time={2}&memory={3}&sim=0&simid=0&pass_rate={4}",
                    sid, result, time, memory, passRate);

            try
            {
                string response = _httpClient.UploadString(_config.TaskFetchUrl, requestBody, 3);
            }
            catch { }
        }
    }
}
