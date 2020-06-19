using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Judger.Managers;
using Judger.Models;

namespace Judger.Core.Program.Internal
{
    /// <summary>
    /// 基于正则的恶意代码检查器
    /// </summary>
    public class CodeChecker
    {
        private const string ConstLangPrefix = "[Language=";

        private readonly Configuration _config = ConfigManager.Config;

        private readonly Dictionary<string, List<string>> _langRulesDic = new Dictionary<string, List<string>>();

        private CodeChecker()
        {
            if (_config.InterceptUnsafeCode)
                InitRulesDictionary();
        }

        public static CodeChecker Instance { get; } = new CodeChecker();

        private void InitRulesDictionary()
        {
            if (!File.Exists(_config.InterceptionRules))
                File.WriteAllText(_config.InterceptionRules, "");

            string rulesData = File.ReadAllText(_config.InterceptionRules);
            string[] rules = Regex.Split(rulesData, "\r\n|\r|\n");

            string nowLang = "";
            foreach (string rule in rules)
            {
                // 空行或注释
                if (string.IsNullOrEmpty(rule) || rule.StartsWith("##"))
                    continue;

                // 语言开始标识
                if (rule.StartsWith(ConstLangPrefix))
                {
                    try
                    {
                        nowLang = rule.Substring(ConstLangPrefix.Length).TrimEnd(']');
                    }
                    catch
                    {
                        // 若此语言的拦截规则编写不规范, 直接忽略当前语言所有规则
                        nowLang = "";
                        LogManager.Error("Can not parse interception rules!");
                    }

                    if (!_langRulesDic.ContainsKey(nowLang))
                        _langRulesDic[nowLang] = new List<string>();

                    continue;
                }

                if (string.IsNullOrEmpty(nowLang))
                    continue;

                _langRulesDic[nowLang].Add(rule);
            }
        }

        /// <summary>
        /// 检查恶意代码
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="language">语言</param>
        /// <param name="unsafeCode">检查出的不安全代码</param>
        /// <param name="lineIndex">不安全代码行号</param>
        /// <returns>是否通过检查</returns>
        public bool Check(string sourceCode, string language, out string unsafeCode, out int lineIndex)
        {
            unsafeCode = "";
            lineIndex = -1;
            if (!_config.InterceptUnsafeCode)
                return true;

            if (!_langRulesDic.ContainsKey(language))
                return true;

            string[] lines = Regex.Split(sourceCode, "\r\n|\r|\n");
            List<string> rules = _langRulesDic[language];
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (string rule in rules)
                {
                    if (Regex.IsMatch(lines[i], rule, RegexOptions.Singleline))
                    {
                        unsafeCode = lines[i];
                        lineIndex = i + 1;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}