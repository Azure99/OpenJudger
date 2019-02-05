using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Judger.Entity;
using Judger.Managers;

namespace Judger.Core.Program
{
    /// <summary>
    /// 基于正则的恶意代码检查器
    /// </summary>
    public class CodeChecker
    {
        /// <summary>
        /// 实例
        /// </summary>
        public static CodeChecker Singleton { get; private set; } = new CodeChecker();

        private Dictionary<string, List<string>> _langRulesDic = new Dictionary<string, List<string>>();

        private Configuration _config = ConfigManager.Config;
        private CodeChecker()
        {
            if(_config.InterceptUnsafeCode)
            {
                InitRulesDictionary();
            }
        }

        private void InitRulesDictionary()
        {
            const string LANG_START = "[Language=";

            if (!File.Exists(_config.InterceptionRules)) 
            {
                File.WriteAllText(_config.InterceptionRules, "");
            }

            string rulesData = File.ReadAllText(_config.InterceptionRules);
            string[] rules = Regex.Split(rulesData, "\r\n|\r|\n");

            string nowLang = "";
            foreach(string rule in rules)
            {
                if (string.IsNullOrEmpty(rule) || rule.StartsWith("##"))//空行或注释
                {
                    continue;
                }

                if(rule.StartsWith(LANG_START))//语言开始标识
                {
                    try
                    {
                        nowLang = rule.Substring(LANG_START.Length).TrimEnd(']');
                    }
                    catch
                    {
                        //若语言信息不规范, 直接忽略当前语言所有规则
                        nowLang = "";
                        LogManager.Error("Can not parse interception rules!");
                    }
                    
                    if (!_langRulesDic.ContainsKey(nowLang)) 
                    {
                        _langRulesDic[nowLang] = new List<string>();
                    }
                    continue;
                }

                if (string.IsNullOrEmpty(nowLang))
                {
                    continue;
                }

                _langRulesDic[nowLang].Add(rule);
            }
        }

        /// <summary>
        /// 检查代码
        /// </summary>
        /// <param name="sourceCode">源代码</param>
        /// <param name="language">语言</param>
        /// <param name="unsafeCode">检查出的不安全代码</param>
        /// <param name="lineIndex">不安全代码行号</param>
        /// <returns>是否安全</returns>
        public bool CheckCode(string sourceCode, string language, out string unsafeCode, out int lineIndex)
        {
            unsafeCode = "";
            lineIndex = -1;
            if (!_config.InterceptUnsafeCode) 
            {
                return true;
            }

            if (!_langRulesDic.ContainsKey(language)) 
            {
                return true;
            }

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
