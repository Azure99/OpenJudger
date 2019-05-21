using System;
using Newtonsoft.Json.Linq;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// 校验Token
    /// </summary>
    public static class Token
    {
        private static readonly Configuration Config = ConfigManager.Config;

        private const string JOBJECT_JUDGER_NAME = "judgerName";
        private const string JOBJECT_TOKEN = "token";

        /// <summary>
        /// 生成校验Token
        /// </summary>
        /// <returns>校验Token</returns>
        public static string Create()
        {
            //Token = MD5 ( MD5( JudgerName + SecretKey ) + UtcDate )

            string name = Config.JudgerName;
            string secret = Config.Password;
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string nameSecretHash = Md5Encrypt.EncryptToHexString(name + secret);
            string token = Md5Encrypt.EncryptToHexString(nameSecretHash + date);

            return token;
        }

        /// <summary>
        /// 创建带有校验字段的JObject
        /// </summary>
        /// <returns>带有校验字段的JObject</returns>
        public static JObject CreateJObject()
        {
            JObject jObject = new JObject();
            AddTokenToJObject(jObject);

            return jObject;
        }

        /// <summary>
        /// 向JObject中添加校验字段
        /// </summary>
        /// <param name="jObject">待添加校验字段的JObject</param>
        public static void AddTokenToJObject(JObject jObject)
        {
            string token = Create();

            jObject.Add(JOBJECT_JUDGER_NAME, Config.JudgerName);
            jObject.Add(JOBJECT_TOKEN, token);
        }
    }
}