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
            }
            else
            {
                Config = SampleJsonSerializaer.DeSerialize<Configuration>(configJson);
            }
            
            SetIsDbConfigField();
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
        /// 设置语言配置中的IsDbConfig字段
        /// </summary>
        private static void SetIsDbConfigField()
        {
            foreach (ILangConfig item in Config.Languages)
            {
                item.IsDbConfig = false;
            }

            foreach (ILangConfig item in Config.Databases)
            {
                item.IsDbConfig = true;
            }
        }

        /// <summary>
        /// 获取语言配置信息
        /// </summary>
        /// <param name="languageName">语言名称</param>
        /// <returns>语言对应的配置信息</returns>
        public static ILangConfig GetLanguageConfig(string languageName)
        {
            ProgramLangConfig[] programConfigs = Config.Languages;
            foreach(var item in programConfigs)
            {
                if(item.Name == languageName)
                {
                    return item.Clone() as ProgramLangConfig;
                }
            }

            DbLangConfig[] dbConfigs = Config.Databases;
            foreach (var item in dbConfigs)
            {
                if(item.Name == languageName)
                {
                    return item.Clone() as DbLangConfig;
                }
            }

            return null;
        }

        public static bool HasLanguage(string languageName)
        {
            return GetLanguageConfig(languageName) != null;
        }

        public static Dictionary<string, ProgramLangConfig> GetLanguageDictionary()
        {
            Dictionary<string, ProgramLangConfig> langDic = new Dictionary<string, ProgramLangConfig>();

            ProgramLangConfig[] languages = ConfigManager.Config.Languages;
            foreach (var lang in languages)
            {
                if (!langDic.ContainsKey(lang.Name))
                {
                    langDic.Add(lang.Name, lang.Clone() as ProgramLangConfig);
                }
            }

            return langDic;
        }

        public static Dictionary<string, ProgramLangConfig> GetLangSourceExtensionDictionary()
        {
            Dictionary<string, ProgramLangConfig> extDic = new Dictionary<string, ProgramLangConfig>();

            ProgramLangConfig[] languages = ConfigManager.Config.Languages;
            foreach (var lang in languages)
            {
                string[] extensions = lang.SourceCodeFileExtension.Split('|');
                foreach(var ex in extensions)
                {
                    if (!extDic.ContainsKey(ex))
                    {
                        extDic.Add(ex, lang.Clone() as ProgramLangConfig);
                    }
                    else
                    {
                        LogManager.Warning(
                            "Source file extension conflict!" + Environment.NewLine +
                            "Extension: " + ex + Environment.NewLine +
                            "Languages: " + lang.Name + ", " + extDic[ex].Name);
                    }
                }
            }

            return extDic;
        }
    }
}
