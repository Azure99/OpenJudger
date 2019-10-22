using System;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;
using Newtonsoft.Json.Linq;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// 校验Token
    /// </summary>
    public static class Token
    {
        private const string JOBJECT_JUDGER_NAME = "judgerName";
        private const string JOBJECT_TOKEN = "token";
        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 生成校验Token
        /// </summary>
        /// <returns>校验Token</returns>
        public static string Create()
        {
            // Token = MD5 ( MD5( JudgerName + Password ) + UtcDate )

            string name = Config.JudgerName;
            string secret = Config.Password;
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string nameAndPasswordHash = Md5Encrypt.EncryptToHexString(name + secret);
            string token = Md5Encrypt.EncryptToHexString(nameAndPasswordHash + date);

            return token;
        }

        /// <summary>
        /// 创建带有校验字段的JObject
        /// </summary>
        /// <returns>带有校验字段的JObject</returns>
        public static JObject CreateJObject()
        {
            var obj = new JObject();
            AddTokenToJObject(obj);

            return obj;
        }

        /// <summary>
        /// 向JObject中添加校验字段
        /// </summary>
        /// <param name="obj">待添加校验字段的JObject</param>
        public static void AddTokenToJObject(JObject obj)
        {
            string token = Create();

            obj.Add(JOBJECT_JUDGER_NAME, Config.JudgerName);
            obj.Add(JOBJECT_TOKEN, token);
        }
    }
}