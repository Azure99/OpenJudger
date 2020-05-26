using System;

namespace Judger.Utils
{
    /// <summary>
    /// String扩展
    /// </summary>
    public static class StringExtends
    {
        /// <summary>
        /// 移除字符串开头的0到N个子字符串
        /// </summary>
        public static string TrimStart(this string str, string trimString)
        {
            int count = 0;
            while (str.IndexOf(trimString, trimString.Length * count, StringComparison.Ordinal)
                % trimString.Length == 0)
                count++;

            return str.Substring(trimString.Length * count);
        }

        /// <summary>
        /// 比较当前字符串与另一字符串
        /// </summary>
        public static int CompareToOrdinal(this string a, string b)
        {
            return string.Compare(a, b, StringComparison.Ordinal);
        }

        public static bool EqualsIgnoreCase(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}