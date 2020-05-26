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
        private readonly HttpWebClient _httpClient = WebClientFactory.Create();
        private readonly string _loginUrl = "http://localhost/judge/login";

        private Authenticator()
        {
            CookieContainer = new CookieContainer();
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
        public CookieContainer CookieContainer { get; }

        private bool Login()
        {
            string requestBody = $"username={_config.JudgerName}&password={_config.Password}";

            try
            {
                string response = _httpClient.UploadString(_loginUrl, requestBody, 3);
                ServerMessageEntity message = Json.DeSerialize<ServerMessageEntity>(response);
                return message.IsSuccess;
            }
            catch
            {
                return false;
            }
        }
    }
}