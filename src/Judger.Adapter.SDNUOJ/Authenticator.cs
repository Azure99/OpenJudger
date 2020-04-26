﻿using System.Net;
using Judger.Adapter.SDNUOJ.Entity;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter.SDNUOJ
{
    public class Authenticator
    {
        private readonly HttpWebClient _httpClient = ConfiguredClient.Create();
        private readonly string _loginUrl = "http://localhost/judge/login";

        private Authenticator()
        {
            CookieContainer = new CookieContainer();
            _httpClient.CookieContainer = CookieContainer;

            if (Config.AdditionalConfigs.ContainsKey("LoginUrl"))
                _loginUrl = Config.AdditionalConfigs["LoginUrl"];
            else
            {
                Config.AdditionalConfigs.Add("LoginUrl", _loginUrl);
                ConfigManager.SaveConfig();
            }

            Login();
        }

        public static Authenticator Instance { get; } = new Authenticator();
        public CookieContainer CookieContainer { get; }
        private Configuration Config { get; } = ConfigManager.Config;

        private bool Login()
        {
            string requestBody = $"username={Config.JudgerName}&password={Config.Password}";

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