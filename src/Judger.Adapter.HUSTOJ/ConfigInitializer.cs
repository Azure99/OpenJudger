using System;
using System.Collections.Generic;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Program;

namespace Judger.Adapter.HUSTOJ
{
    public class ConfigInitializer : IConfigInitializer
    {
        public void Init()
        {
            Console.Write("Input HUSTOJ url (http://localhost:8080/): ");

            string baseUrl = Console.ReadLine();
            if (baseUrl != null && !baseUrl.EndsWith("/"))
                baseUrl += "/";

            string judgeUrl = baseUrl + "admin/problem_judge.php";
            string loginUrl = baseUrl + "login.php";

            Console.Write("Input judger username (need http_judge): ");
            string username = Console.ReadLine();
            Console.Write("Input judger password: ");
            string password = Console.ReadLine();

            Configuration config = ConfigManager.Config;
            config.AdapterDllPath = "Judger.Adapter.HUSTOJ.dll";
            config.TaskFetchUrl = judgeUrl;
            config.TestDataFetchUrl = judgeUrl;
            config.ResultSubmitUrl = judgeUrl;
            config.AdditionalConfigs["LoginUrl"] = loginUrl;
            config.JudgerName = username;
            config.Password = password;
            ConvertLanguageName(config);

            ConfigManager.SaveConfig();
        }

        private void ConvertLanguageName(Configuration config)
        {
            Dictionary<string, string> langDic = new Dictionary<string, string>
            {
                {"c", "0"},
                {"cpp", "1"},
                {"java", "3"},
                {"python", "6"}
            };

            foreach (ProgramLangConfig langConfig in config.Languages)
            {
                if (langDic.ContainsKey(langConfig.Name))
                    langConfig.Name = langDic[langConfig.Name];
            }
        }
    }
}