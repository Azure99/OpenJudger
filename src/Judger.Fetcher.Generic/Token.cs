using System;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// 校验Token
    /// </summary>
    public static class Token
    {
        /// <summary>
        /// 生成校验Token
        /// </summary>
        public static string Create()
        {
            //Token = MD5( MD5( JudgerName + SecretKey ) + UtcDate )

            string name = ConfigManager.Config.JudgerName;
            string secret = ConfigManager.Config.Password;
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");

            string nameSecret = Md5Encrypt.EncryptToHexString(name + secret);
            string token = Md5Encrypt.EncryptToHexString(nameSecret + date);

            return token;
        }
    }
}
