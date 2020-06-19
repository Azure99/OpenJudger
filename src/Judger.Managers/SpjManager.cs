using System;
using System.Collections.Generic;
using System.IO;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Managers
{
    /// <summary>
    /// Special Judge管理器
    /// </summary>
    public static class SpjManager
    {
        /*
         * 文件名示例
         * program.cpp.exe
         */
        private const string ConstSpjProgramFilename = "program";
        private const string ConstSpjSourceFilename = "source";
        private const string ConstSpjTestDataDir = "spj";
        private const string ConstSpjDirectory = "spj";

        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// SPJ程序源文件名(无扩展名)
        /// </summary>
        public static string SpjSourceFilename => ConstSpjSourceFilename;

        /// <summary>
        /// 获取语言名-源文件扩展名字典
        /// </summary>
        /// <returns>语言名-源文件扩展名字典</returns>
        public static Dictionary<string, ProgramLangConfig> GetLangSourceExtensionDictionary()
        {
            Dictionary<string, ProgramLangConfig> extDic = new Dictionary<string, ProgramLangConfig>();

            ProgramLangConfig[] languages = Config.Languages;
            foreach (ProgramLangConfig lang in languages)
            {
                string[] extensions = lang.SourceCodeFileExtension.Split('|');
                foreach (string ex in extensions)
                {
                    if (!extDic.ContainsKey(ex))
                        extDic.Add(ex, lang.Clone() as ProgramLangConfig);
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

        /// <summary>
        /// 获取评测目录下的SPJ目录
        /// </summary>
        /// <param name="context">评测上下文</param>
        /// <returns>SPJ目录</returns>
        public static string GetSpjDirectoryInJudger(JudgeContext context)
        {
            if (context.TempDirectory.EndsWith(ConstSpjDirectory))
                return context.TempDirectory;

            return Path.Combine(context.TempDirectory, ConstSpjDirectory);
        }

        /// <summary>
        /// 获取测试数据目录下的SPJ目录
        /// </summary>
        /// <param name="problemId">题目Id</param>
        /// <returns>SPJ目录</returns>
        public static string GetSpjDirectoryInTestData(string problemId)
        {
            return Path.Combine(Config.TestDataDirectory, problemId, ConstSpjTestDataDir);
        }

        /// <summary>
        /// 获取评测目录下的SPJ程序的源文件路径
        /// </summary>
        /// <returns>SPJ源文件名</returns>
        public static string GetSpjSourceFileInJudger(JudgeContext context)
        {
            return Path.Combine(
                GetSpjDirectoryInJudger(context),
                ((ProgramLangConfig)context.LangConfig).SourceCodeFileName);
        }

        /// <summary>
        /// 寻找测试数据目录下的SPJ源文件
        /// </summary>
        /// <returns>SPJ源文件路径</returns>
        public static string FindSpjSourceFileInTestData(string problemId, int index = 0)
        {
            Dictionary<string, ProgramLangConfig> extDic = GetLangSourceExtensionDictionary();
            string spjDirectory = GetSpjDirectoryInTestData(problemId);

            string[] files = Directory.GetFiles(spjDirectory);
            List<string> spjSourceFiles = new List<string>();
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                string fileExt = Path.GetExtension(file).TrimStart('.').ToLower();
                if (fileName == ConstSpjSourceFilename && extDic.ContainsKey(fileExt))
                    spjSourceFiles.Add(PathHelper.GetBaseAbsolutePath(file));
            }

            if (index < 0 || index >= spjSourceFiles.Count)
                return null;

            return spjSourceFiles[index];
        }

        /// <summary>
        /// 寻找评测目录下的SPJ程序
        /// </summary>
        /// <returns>SPJ程序路径</returns>
        public static string FindSpjProgramInJudger(JudgeContext context)
        {
            string compileDirectory = GetSpjDirectoryInJudger(context);
            string[] files = Directory.GetFiles(compileDirectory);

            if (files.Length > 2)
                LogManager.Warning("Can not confirm the unique special judge program!");

            string spjProgramPath = null;
            foreach (string file in files)
            {
                if (Path.GetFileName(file) == ((ProgramLangConfig)context.LangConfig).SourceCodeFileName)
                    continue;

                spjProgramPath = file;
            }

            return PathHelper.GetBaseAbsolutePath(spjProgramPath);
        }

        /// <summary>
        /// 寻找测试数据目录下的SPJ程序
        /// </summary>
        /// <returns>SPJ程序</returns>
        public static string FindSpjProgramInTestData(string problemId, int index = 0)
        {
            Dictionary<string, ProgramLangConfig> extDic = GetLangSourceExtensionDictionary();
            string spjDirectory = GetSpjDirectoryInTestData(problemId);

            List<string> spjSourceFiles = new List<string>();
            string[] files = Directory.GetFiles(spjDirectory);
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                string fileExt = Path.GetExtension(fileName).TrimStart('.');
                fileName = Path.GetFileNameWithoutExtension(fileName);

                if (fileName == ConstSpjProgramFilename && extDic.ContainsKey(fileExt))
                    spjSourceFiles.Add(PathHelper.GetBaseAbsolutePath(file));
            }

            if (index < 0 || index >= spjSourceFiles.Count)
                return null;

            return spjSourceFiles[index];
        }

        /// <summary>
        /// 获取评测目录下的SPJ程序路径
        /// </summary>
        /// <returns>SPJ程序路径</returns>
        public static string GetSpjProgramPathInJudger(JudgeContext context)
        {
            string spjDirectory = GetSpjDirectoryInJudger(context);
            string path = Path.Combine(spjDirectory, ((ProgramLangConfig)context.LangConfig).ProgramFileName);

            return PathHelper.GetBaseAbsolutePath(path);
        }

        /// <summary>
        /// 获取测试数据目录下的SPJ程序路径
        /// </summary>
        /// <returns>SPJ程序路径</returns>
        public static string GetSpjProgramPathInTestData(string problemId, ProgramLangConfig lang)
        {
            string spjDirectory = GetSpjDirectoryInTestData(problemId);
            string programExt = Path.GetExtension(lang.ProgramFileName)?.TrimStart('.').ToLower();
            string sourceExt = lang.SourceCodeFileExtension.Split('|')[0].ToLower();
            string path = Path.Combine(spjDirectory, ConstSpjProgramFilename + "." + sourceExt + "." + programExt);

            return PathHelper.GetBaseAbsolutePath(path);
        }

        /// <summary>
        /// 创建SPJ的JudgeContext
        /// </summary>
        /// <param name="originContext">源评测任务</param>
        /// <returns>SPJ的JudgeTask</returns>
        /// 用于编译运行SPJ程序
        public static JudgeContext CreateSpjJudgeContext(JudgeContext originContext)
        {
            JudgeContext newContext = (JudgeContext) originContext.Clone();

            string spjSourceFilePath = FindSpjSourceFileInTestData(newContext.Task.ProblemId);
            if (spjSourceFilePath == null) //没有SPJ程序源代码, 无法评测
                throw new JudgeException("No special judge program exception!");

            newContext.Task.SourceCode = File.ReadAllText(spjSourceFilePath);

            newContext.LangConfig = GetLangConfigBySourceFilePath(spjSourceFilePath);
            ProgramLangConfig langConfig = (ProgramLangConfig) newContext.LangConfig;
            newContext.Task.Language = newContext.LangConfig.Name;

            string spjDir = GetSpjDirectoryInJudger(originContext) + "\\";
            string appDir = PathHelper.GetBaseAbsolutePath("");

            // 替换<tempdir>和<appdir>字段
            langConfig.CompilerPath = langConfig.CompilerPath.Replace("<tempdir>", spjDir).Replace("<appdir>", appDir);
            langConfig.CompilerWorkDirectory = langConfig.CompilerWorkDirectory.Replace("<tempdir>", spjDir)
                .Replace("<appdir>", appDir);
            langConfig.CompilerArgs = langConfig.CompilerArgs.Replace("<tempdir>", spjDir).Replace("<appdir>", appDir);
            langConfig.RunnerPath = langConfig.RunnerPath.Replace("<tempdir>", spjDir).Replace("<appdir>", appDir);
            langConfig.RunnerWorkDirectory = langConfig.RunnerWorkDirectory.Replace("<tempdir>", spjDir)
                .Replace("<appdir>", appDir);
            langConfig.RunnerArgs = langConfig.RunnerArgs.Replace("<tempdir>", spjDir).Replace("<appdir>", appDir);

            newContext.TempDirectory = spjDir;

            if (!Directory.Exists(spjDir))
                Directory.CreateDirectory(spjDir);

            return newContext;
        }

        /// <summary>
        /// 根据程序路径获取其关联的语言配置
        /// </summary>
        /// <returns>语言配置</returns>
        public static ProgramLangConfig GetLangConfigByProgramPath(string path)
        {
            Dictionary<string, ProgramLangConfig> extDic = GetLangSourceExtensionDictionary();

            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            string fileExt = Path.GetExtension(fileName).TrimStart('.');

            return extDic.ContainsKey(fileExt) ? extDic[fileExt] : null;
        }

        /// <summary>
        /// 根据SPJ源文件路径获取其关联的语言配置
        /// </summary>
        /// <returns>语言配置</returns>
        public static ProgramLangConfig GetLangConfigBySourceFilePath(string path)
        {
            Dictionary<string, ProgramLangConfig> extDic = GetLangSourceExtensionDictionary();

            string fileExt = Path.GetExtension(path).TrimStart('.').ToLower();

            return extDic.ContainsKey(fileExt) ? extDic[fileExt] : null;
        }
    }
}