using System.Net;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Utils;

namespace Judger.Adapter.HUSTOJ
{
    public class Authenticator
    {
        private readonly Configuration _config = ConfigManager.Config;
        private readonly HttpWebClient _httpClient = HttpClientFactory.Create();
        private readonly string _loginUrl = "http://localhost/login.php";

        private Authenticator()
        {
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

        public static Authenticator Instance { get; } = new Authenticator();
        public CookieContainer CookieContainer { get; } = new CookieContainer();

        private void Login()
        {
            string requestBody = $"user_id={_config.JudgerName}&password={_config.Password}";
            _httpClient.UploadString(_loginUrl, requestBody, 3);

            if (CheckLogin())
                return;

            throw new AdapterException("Maybe no http_judge privilege!\n" +
                                       "Please check your judger username, password and privilege!");
        }

        private bool CheckLogin()
        {
            return _httpClient.UploadString(_config.TaskFetchUrl, "checklogin=1") == "1";
        }
    }
}