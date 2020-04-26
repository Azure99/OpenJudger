using System;

namespace Judger.Utils
{
    /// <summary>
    /// String扩展类
    /// </summary>
    public static class StringExtends
    {
        /// <summary>
        /// 移除字符串开头的0到N个子字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="trimString">子字符串</param>
        /// <returns>移除子字符串后的字符串</returns>
        public static string TrimStart(this string str, string trimString)
        {
            int count = 0;
            while (str.IndexOf(trimString, trimString.Length * count, StringComparison.Ordinal)
                   % trimString.Length == 0)
                count++;

            return str.Substring(trimString.Length * count);
        }
    }
}