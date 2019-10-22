﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Judger.Models;
using Judger.Models.Database;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Managers
{
    /// <summary>
    /// 配置信息管理器
    /// </summary>
    public static class ConfigManager
    {
        static ConfigManager()
        {
            FileHelper.TryReadAllText("Config.json", out string configJson);

            if (string.IsNullOrEmpty(configJson)) //创建新配置文件
            {
                Config = new Configuration
                {
                    Languages = GetDefaultLangConfigs(),
                    Databases = GetDefaultDbConfigs(),
                    Password = RandomString.Next(16)
                };
                Config.AdditionalConfigs.Add("SampleKey", "SampleValue");
            }
            else
            {
                Config = SampleJsonSerializer.DeSerialize<Configuration>(configJson);
            }

            SetIsDbConfigField();
            SaveConfig();
        }

        /// <summary>
        /// 配置信息实例
        /// </summary>
        public static Configuration Config { get; }

        /// <summary>
        /// 设置语言配置中的IsDbConfig字段
        /// </summary>
        private static void SetIsDbConfigField()
        {
            foreach (ILangConfig item in Config.Languages)
                item.IsDbConfig = false;

            foreach (ILangConfig item in Config.Databases)
                item.IsDbConfig = true;
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        public static void SaveConfig()
        {
            FileHelper.TryWriteAllText("Config.json", SampleJsonSerializer.Serialize(Config));
        }

        /// <summary>
        /// 获取语言配置信息
        /// </summary>
        /// <param name="languageName">语言名称</param>
        /// <returns>语言对应的配置信息</returns>
        public static ILangConfig GetLanguageConfig(string languageName)
        {
            ProgramLangConfig[] programConfigs = Config.Languages;
            foreach (ProgramLangConfig item in programConfigs)
            {
                if (item.Name == languageName)
                    return item.Clone() as ProgramLangConfig;
            }

            

            DbLangConfig[] dbConfigs = Config.Databases;
            foreach (DbLangConfig item in dbConfigs)
            {
                if (item.Name == languageName)
                    return item.Clone() as DbLangConfig;
            }

            return Config.Databases
                .Where(i => i.Name == languageName)
                .Select(i => i.Clone() as DbLangConfig)
                .FirstOrDefault();
        }

        /// <summary>
        /// 获取默认编程语言配置
        /// </summary>
        /// <returns>编程语言配置</returns>
        private static ProgramLangConfig[] GetDefaultLangConfigs()
        {
            char sparChar = Path.DirectorySeparatorChar;

            var langConfigs = new List<ProgramLangConfig>();

            var c = new ProgramLangConfig
            {
                Name = "c",
                IsDbConfig = false,
                NeedCompile = true,
                RunningInVm = false,
                SourceCodeFileName = "src.c",
                SourceCodeFileExtension = "c",
                ProgramFileName = "program.exe",
                UseUtf8 = true,
                MaxCompileTime = 20000,
                JudgeDirectory = "JudgeTemp" + sparChar + "CJudge",
                CompilerPath = "gcc",
                CompilerWorkDirectory = "<tempdir>",
                CompilerArgs = "src.c -o program.exe",
                RunnerPath = "<tempdir>program.exe",
                RunnerWorkDirectory = "<tempdir>",
                RunnerArgs = "",
                OutputLimit = 67108864,
                TimeCompensation = 1.0
            };

            var cpp = new ProgramLangConfig
            {
                Name = "cpp",
                IsDbConfig = false,
                NeedCompile = true,
                RunningInVm = false,
                SourceCodeFileName = "src.cpp",
                SourceCodeFileExtension = "cc|cpp",
                ProgramFileName = "program.exe",
                UseUtf8 = true,
                MaxCompileTime = 20000,
                JudgeDirectory = "JudgeTemp" + sparChar + "CppJudge",
                CompilerPath = "g++",
                CompilerWorkDirectory = "<tempdir>",
                CompilerArgs = "src.cpp -o program.exe",
                RunnerPath = "<tempdir>program.exe",
                RunnerWorkDirectory = "<tempdir>",
                RunnerArgs = "",
                OutputLimit = 67108864,
                TimeCompensation = 1.0
            };


            var java = new ProgramLangConfig
            {
                Name = "java",
                IsDbConfig = false,
                NeedCompile = true,
                RunningInVm = true,
                SourceCodeFileName = "Main.java",
                SourceCodeFileExtension = "java",
                ProgramFileName = "Main.class",
                UseUtf8 = false,
                MaxCompileTime = 30000,
                JudgeDirectory = "JudgeTemp" + sparChar + "JavaJudge",
                CompilerPath = "javac",
                CompilerWorkDirectory = "<tempdir>",
                CompilerArgs = "-encoding utf-8 Main.java",
                RunnerPath = "java",
                RunnerWorkDirectory = "<tempdir>",
                RunnerArgs = "Main",
                OutputLimit = 67108864,
                TimeCompensation = 1.0
            };

            var python = new ProgramLangConfig
            {
                Name = "python",
                IsDbConfig = false,
                NeedCompile = false,
                RunningInVm = true,
                SourceCodeFileName = "src.py",
                SourceCodeFileExtension = "py",
                ProgramFileName = "src.py",
                UseUtf8 = true,
                MaxCompileTime = 20000,
                JudgeDirectory = "JudgeTemp" + sparChar + "PythonJudge",
                CompilerPath = "",
                CompilerWorkDirectory = "",
                CompilerArgs = "",
                RunnerPath = "python",
                RunnerWorkDirectory = "<tempdir>",
                RunnerArgs = "<tempdir>src.py",
                OutputLimit = 67108864,
                TimeCompensation = 1.0
            };

            langConfigs.Add(c);
            langConfigs.Add(cpp);
            langConfigs.Add(java);
            langConfigs.Add(python);

            return langConfigs.ToArray();
        }

        /// <summary>
        /// 获取默认数据库配置
        /// </summary>
        /// <returns>数据库配置</returns>
        private static DbLangConfig[] GetDefaultDbConfigs()
        {
            var langConfigs = new List<DbLangConfig>();

            var mysql = new DbLangConfig
            {
                Name = "mysql",
                IsDbConfig = true,
                DriverPath = "Pomelo.Data.MySql.dll",
                Server = "localhost",
                Database = "judger",
                User = "root",
                Password = "123456",
                ConnStringTemplate = "Server=<Server>;Database=<Database>;User=<User>;Password=<Password>;CharSet=utf8;"
            };

            langConfigs.Add(mysql);

            return langConfigs.ToArray();
        }
    }
}