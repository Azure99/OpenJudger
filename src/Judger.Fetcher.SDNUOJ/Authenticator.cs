using System.Net;
using Judger.Fetcher.SDNUOJ.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// SDNUOJ评测机认证
    /// </summary>
    public class Authenticator
    {
        private readonly HttpWebClient _httpClient = ConfiguredClient.Create();
        private readonly string _loginUrl = "http://localhost/judge/login";

        private Authenticator()
        {
            CookieContainer = new CookieContainer();
            _httpClient.CookieContainer = CookieContainer;

            if (Config.AdditionalConfigs.ContainsKey("LoginUrl"))
            {
                _loginUrl = Config.AdditionalConfigs["LoginUrl"];
            }
            else
            {
                Config.AdditionalConfigs.Add("LoginUrl", _loginUrl);
                ConfigManager.SaveConfig();
            }

            Login();
        }

        public static Authenticator Instance { get; } = new Authenticator();

        // 保存Cookie
        public CookieContainer CookieContainer { get; }
        private Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 登录SDNUOJ
        /// </summary>
        public bool Login()
        {
            string requestBody = $"username={Config.JudgerName}&password={Config.Password}";

            try
            {
                string response = _httpClient.UploadString(_loginUrl, requestBody, 3);

                var message = SampleJsonSerializer.DeSerialize<ServerMessageEntity>(response);
                return message.IsSuccess;
            }
            catch
            {
                return false;
            }
        }
    }
}