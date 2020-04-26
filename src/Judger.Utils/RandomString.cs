using System;
using System.Collections.Generic;
using System.Text;

namespace Judger.Utils
{
    /// <summary>
    /// 随机字符串辅助类
    /// </summary>
    public static class RandomString
    {
        // 字符表
        private static readonly char[] Chars;

        static RandomString()
        {
            List<char> charsList = new List<char>();

            for (int i = 0; i <= 9; i++)
                charsList.Add((char) ('0' + i));

            for (int i = 0; i < 26; i++)
            {
                charsList.Add((char) ('a' + i));
                charsList.Add((char) ('A' + i));
            }

            Chars = charsList.ToArray();
        }

        /// <summary>
        /// 生成大小写字母与数字混合的随机字符串
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机字符串</returns>
        public static string Next(int length)
        {
            Random random = new Random();
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                char chr = Chars[random.Next(0, Chars.Length - 1)];
                builder.Append(chr);
            }

            return builder.ToString();
        }
    }
}