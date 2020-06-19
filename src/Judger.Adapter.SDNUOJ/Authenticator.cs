using System.Net;
using Judger.Adapter.SDNUOJ.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter.SDNUOJ
{
    public class Authenticator
    {
        private readonly Configuration _config = ConfigManager.Config;
        private readonly HttpWebClient _httpClient = HttpClientFactory.Create();
        private readonly string _loginUrl = "http://localhost/judge/login";

        private Authenticator()
        {
            _httpClient.CookieContainer = CookieContainer;

            if (_config.AdditionalConfigs.ContainsKey("LoginUrl"))
                _loginUrl = _config.AdditionalConfigs["LoginUrl"];
            else
            {
                _config.AdditionalConfigs.Add("LoginUrl", _loginUrl);
                ConfigManager.SaveConfig();
            }

            Login();
        }

        public static Authenticator Instance { get; } = new Authenticator();
        public CookieContainer CookieContainer { get; } = new CookieContainer();

        private void Login()
        {
            string requestBody = $"username={_config.JudgerName}&password={_config.Password}";
            string jsonResp = _httpClient.UploadString(_loginUrl, requestBody, 3);
            ServerMessage message = Json.DeSerialize<ServerMessage>(jsonResp);

            if (!message.IsSuccess)
                LogManager.Error("Login failed, please check password!");
        }
    }
}