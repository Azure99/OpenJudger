using System.Security.Cryptography;
using System.Text;

namespace Judger.Utils
{
    /// <summary>
    /// MD5辅助类
    /// </summary>
    public static class Md5Encrypt
    {
        /// <summary>
        /// 计算String的MD5(小写32位)
        /// </summary>
        public static string EncryptToHexString(string str)
        {
            str ??= "";

            byte[] result;
            using (MD5 md5 = MD5.Create())
            {
                result = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            }

            StringBuilder builder = new StringBuilder();
            foreach (byte b in result)
                builder.Append(b.ToString("x2"));

            return builder.ToString();
        }
    }
}