using System;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic
{
    public static class TokenUtil
    {
        private const string ConstJudgerName = "judgerName";
        private const string ConstToken = "token";
        private static Configuration Config { get; } = ConfigManager.Config;

        public static JObject CreateJObject()
        {
            JObject obj = new JObject();

            string token = ComputeToken();
            obj.Add(ConstJudgerName, Config.JudgerName);
            obj.Add(ConstToken, token);

            return obj;
        }

        private static string ComputeToken()
        {
            // Token = MD5(MD5(JudgerName + Password) + UtcDate)

            string name = Config.JudgerName;
            string secret = Config.Password;
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string namePasswordHash = Md5Encrypt.EncryptToHexString(name + secret);
            string token = Md5Encrypt.EncryptToHexString(namePasswordHash + date);

            return token;
        }
    }
}