using System;
using System.Net;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter.HUSTOJ
{
    public class Authenticator
    {
        private readonly Configuration _config = ConfigManager.Config;
        private readonly HttpWebClient _httpClient = WebClientFactory.Create();
        private readonly string _loginUrl = "http://localhost/login.php";
        private int _delayCheckCount;

        static Authenticator()
        {
            Instance = new Authenticator();
        }

        private Authenticator()
        {
            CookieContainer = new CookieContainer();
            _httpClient.CookieContainer = CookieContainer;

            if (_config.AdditionalConfigs.ContainsKey("LoginUrl"))
                _loginUrl = _config.AdditionalConfigs["LoginUrl"];
            else
            {
                _config.AdditionalConfigs["LoginUrl"] = _loginUrl;
                ConfigManager.SaveConfig();
            }

            Login();
        }

        public static Authenticator Instance { get; }
        public CookieContainer CookieContainer { get; }

        private bool Login()
        {
            if (CheckLogin())
                return true;

            string requestBody = $"user_id={_config.JudgerName}&password={_config.Password}";
            try
            {
                _httpClient.UploadString(_loginUrl, requestBody, 3);
            }
            catch (Exception ex)
            {
                LogManager.Exception(ex);
            }

            if (CheckLogin())
                return true;
            else
            {
                Console.WriteLine("No http_judge privilege!");
                Console.WriteLine("Please check your judge username, password and privilege!");
                return false;
            }
        }

        public bool CheckLogin()
        {
            string requestBody = "checklogin=1";
            try
            {
                return _httpClient.UploadString(_config.TaskFetchUrl, requestBody) == "1";
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
            if (_delayCheckCount++ <= 10)
                return;
            _delayCheckCount = 0;
            Login();
        }

        public void UpdateSolution(string sid, int result, int time, int memory, double passRate)
        {
            string requestBody =
                $"update_solution=1&sid={sid}&result={result}&time={time}" +
                $"&memory={memory}&sim=0&simid=0&pass_rate={passRate}";

            try
            {
                _httpClient.UploadString(_config.TaskFetchUrl, requestBody, 3);
            }
            catch (Exception ex)
            {
                LogManager.Exception(ex);
            }
        }
    }
}