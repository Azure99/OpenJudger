using System;

namespace Judger.Utils
{
    public static class StringExtends
    {
        public static string TrimStart(this string str, string trimString)
        {
            int count = 0;
            while ((str.IndexOf(trimString, trimString.Length * count, StringComparison.Ordinal) % trimString.Length) == 0)
            {
                count++;
            }

            return str.Substring(trimString.Length * count);
        }
    }
}
