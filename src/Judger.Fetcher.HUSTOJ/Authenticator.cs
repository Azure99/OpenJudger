using System.Net;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.HUSTOJ
{
    public class Authenticator
    {
        private int _delayCheckCount;
        private readonly HttpWebClient _httpClient = ConfiguredClient.Create();
        private readonly string loginUrl = "http://localhost/login.php";

        static Authenticator()
        {
            Instance = new Authenticator();
        }

        private Authenticator()
        {
            CookieContainer = new CookieContainer();
            _httpClient.CookieContainer = CookieContainer;

            if (Config.AdditionalConfigs.ContainsKey("LoginUrl"))
            {
                loginUrl = Config.AdditionalConfigs["LoginUrl"];
            }
            else
            {
                Config.AdditionalConfigs["LoginUrl"] = loginUrl;
                ConfigManager.SaveConfig();
            }

            Login();
        }

        public static Authenticator Instance { get; }

        public CookieContainer CookieContainer { get; }

        private Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 登录HUSTOJ
        /// </summary>
        public bool Login()
        {
            if (CheckLogin())
                return true;

            string requestBody = $"user_id={Config.JudgerName}&password={Config.Password}";
            try
            {
                _httpClient.UploadString(loginUrl, requestBody, 3);
            }
            catch
            { }

            return CheckLogin();
        }

        /// <summary>
        /// 检查是否已登录
        /// </summary>
        public bool CheckLogin()
        {
            var requestBody = "checklogin=1";
            try
            {
                string response = _httpClient.UploadString(Config.TaskFetchUrl, requestBody);
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
            if (_delayCheckCount++ <= 10)
                return;
            _delayCheckCount = 0;
            Login();
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
                $"update_solution=1&sid={sid}&result={result}&time={time}" +
                $"&memory={memory}&sim=0&simid=0&pass_rate={passRate}";

            try
            {
                _httpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);
            }
            catch
            { }
        }
    }
}