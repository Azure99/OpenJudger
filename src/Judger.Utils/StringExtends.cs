using System;

namespace Judger.Utils
{
    /// <summary>
    /// String扩展
    /// </summary>
    public static class StringExtends
    {
        /// <summary>
        /// 移除字符串开头的任意个子字符串
        /// </summary>
        public static string TrimStart(this string str, string trim)
        {
            int count = 0;
            while (str.IndexOf(trim, trim.Length * count, StringComparison.Ordinal) % trim.Length == 0)
                count++;

            return str.Substring(trim.Length * count);
        }

        /// <summary>
        /// 对比当前字符串与另一字符串
        /// </summary>
        public static int CompareToOrdinal(this string str, string target)
        {
            return string.Compare(str, target, StringComparison.Ordinal);
        }

        /// <summary>
        /// 比较当前字符串与另一字符串是否相等 (忽略大小写)
        /// </summary>
        public static bool EqualsIgnoreCase(this string str, string target)
        {
            return string.Equals(str, target, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}