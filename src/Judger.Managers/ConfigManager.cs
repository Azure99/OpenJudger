using System;
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
                    return item;
                }
            }

            return null;
        }
    }
}
