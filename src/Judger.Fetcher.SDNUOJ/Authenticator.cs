using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;
using Judger.Fetcher.SDNUOJ.Models;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// SDNUOJ评测机认证
    /// </summary>
    public class Authenticator
    {
        public static Authenticator Singleton { get; private set; } = new Authenticator();

        //保存Cookie
        public CookieContainer CookieContainer { get; private set; }

        private HttpWebClient _webClient = ConfiguredClient.Create();
        private Configuration _config = ConfigManager.Config;
        private string loginUrl = "http://localhost/judge/login";

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
                _config.AdditionalConfig.Add("LoginUrl", loginUrl);
                ConfigManager.SaveConfig();
            }

            Login();
        }

        public bool Login()
        {
            string requestBody = string.Format("username={0}&password={1}", _config.JudgerName, _config.Password);

            try
            {
                string result = _webClient.UploadString(loginUrl, requestBody, 3);

                ServerMessage message = SampleJsonSerializaer.DeSerialize<ServerMessage>(result);
                return message.IsSuccessful;
            }
            catch
            {
                return false;
            }
        }
    }

}
