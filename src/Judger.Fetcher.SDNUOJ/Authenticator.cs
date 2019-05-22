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
        private HttpWebClient _httpClient = ConfiguredClient.Create();
        private string loginUrl = "http://localhost/judge/login";

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
                Config.AdditionalConfigs.Add("LoginUrl", loginUrl);
                ConfigManager.SaveConfig();
            }

            Login();
        }

        public static Authenticator Singleton { get; } = new Authenticator();

        // 保存Cookie
        public CookieContainer CookieContainer { get; }
        private Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 登录SDNUOJ
        /// </summary>
        public bool Login()
        {
            string requestBody = string.Format("username={0}&password={1}", Config.JudgerName, Config.Password);

            try
            {
                string response = _httpClient.UploadString(loginUrl, requestBody, 3);

                ServerMessageEntity message = SampleJsonSerializer.DeSerialize<ServerMessageEntity>(response);
                return message.IsSuccess;
            }
            catch
            {
                return false;
            }
        }
    }
}