using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.HUSTOJ
{
    public class Authenticator
    {
        public static Authenticator Singleton { get; set; }

        public CookieContainer CookieContainer { get; private set; }
        private HttpWebClient _webClient = ConfiguredClient.Create();

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
            _webClient.CookieContainer = CookieContainer;

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


        public bool Login()
        {
            if (CheckLogin())
            {
                return true;
            }

            string body = string.Format("user_id={0}&password={1}", _config.JudgerName, _config.Password);
            try
            {
                string res = _webClient.UploadString(loginUrl, body, 3);
            }
            catch { }

            return CheckLogin();
        }

        public bool CheckLogin()
        {
            string body = "checklogin=1";
            try
            {
                string res = _webClient.UploadString(_config.TaskFetchUrl, body);
                return res == "1";
            }
            catch
            {
                return false;
            }
        }

        public void CheckAndLogin()
        {
            if(delayCheckCount++ > 10)
            {
                delayCheckCount = 0;
                Login();
            }
        }

        public void UpdateSolution(int sid, int result, int time, int memory, double passRate)
        {
            string requestBody = 
                string.Format("update_solution=1&sid={0}&result={1}&time={2}&memory={3}&sim=0&simid=0&pass_rate={4}",
                    sid, result, time, memory, passRate);

            try
            {
                string res = _webClient.UploadString(_config.TaskFetchUrl, requestBody, 3);
            }
            catch { }
        }
    }
}
