using System;
using System.Collections.Generic;
using Judger.Entity;
using Judger.Utils;

namespace Judger.Managers
{
    /// <summary>
    /// 配置信息管理器
    /// </summary>
    public static class ConfigManager
    {
        /// <summary>
        /// 配置信息实例
        /// </summary>
        public static Configuration Config { get; } 
        static ConfigManager()
        {
            FileHelper.TryReadAllText("Config.json", out string configJson);
            if(string.IsNullOrEmpty(configJson))//创建新配置文件
            {
                Config = new Configuration();
                Config.Password = RandomString.Next(16);
                Config.AdditionalConfig.Add("SampleKey", "SampleValue");
                SaveConfig();
                return;
            }

            Config = SampleJsonSerializaer.DeSerialize<Configuration>(configJson);
            SaveConfig();
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        public static void SaveConfig()
        {
            FileHelper.TryWriteAllText("Config.json", SampleJsonSerializaer.Serialize<Configuration>(Config));
        }

        /// <summary>
        /// 获取语言配置信息
        /// </summary>
        /// <param name="languageName">语言名称</param>
        /// <returns>语言对应的配置信息</returns>
        public static LanguageConfiguration GetLanguageConfig(string languageName)
        {
            LanguageConfiguration[] configs = Config.Languages;
            foreach(var item in configs)
            {
                if(item.Language == languageName)
                {
                    return item.Clone() as LanguageConfiguration;
                }
            }

            return null;
        }

        public static bool HasLanguage(string languageName)
        {
            return GetLanguageConfig(languageName) != null;
        }

        public static Dictionary<string, LanguageConfiguration> GetLanguageDictionary()
        {
            Dictionary<string, LanguageConfiguration> langDic = new Dictionary<string, LanguageConfiguration>();

            LanguageConfiguration[] languages = ConfigManager.Config.Languages;
            foreach (var lang in languages)
            {
                if (!langDic.ContainsKey(lang.Language))
                {
                    langDic.Add(lang.Language, lang.Clone() as LanguageConfiguration);
                }
            }

            return langDic;
        }

        public static Dictionary<string, LanguageConfiguration> GetLangSourceExtensionDictionary()
        {
            Dictionary<string, LanguageConfiguration> extDic = new Dictionary<string, LanguageConfiguration>();

            LanguageConfiguration[] languages = ConfigManager.Config.Languages;
            foreach (var lang in languages)
            {
                string[] extensions = lang.SourceCodeFileExtension.Split('|');
                foreach(var ex in extensions)
                {
                    if (!extDic.ContainsKey(ex))
                    {
                        extDic.Add(ex, lang.Clone() as LanguageConfiguration);
                    }
                    else
                    {
                        LogManager.Warning(
                            "Source file extension conflict!" + Environment.NewLine +
                            "Extension: " + ex + Environment.NewLine +
                            "Languages: " + lang.Language + ", " + extDic[ex].Language);
                    }
                }
            }

            return extDic;
        }
    }
}
