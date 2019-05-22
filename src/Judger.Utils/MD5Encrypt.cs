using System.Security.Cryptography;
using System.Text;

namespace Judger.Utils
{
    /// <summary>
    /// MD5加密辅助类
    /// </summary>
    public static class Md5Encrypt
    {
        /// <summary>
        /// 计算String的MD5(小写32位)
        /// </summary>
        /// <param name="str">欲加密的string</param>
        /// <returns>计算结果</returns>
        public static string EncryptToHexString(string str)
        {
            if (str == null)
                str = "";

            byte[] res;
            using (MD5 md5 = MD5.Create())
            {
                res = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            }

            StringBuilder sb = new StringBuilder();
            foreach (byte b in res)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}