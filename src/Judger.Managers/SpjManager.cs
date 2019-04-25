using System.Collections.Generic;
using System.IO;
using Judger.Entity;
using Judger.Entity.Program;
using Judger.Utils;

namespace Judger.Managers
{
    public static class SpjManager
    {
        //program.cpp.exe
        public const string SPJ_PROGRAM_FILENAME = "program";
        //source.cpp
        public const string SPJ_SOURCE_FILENAME = "source";
        public const string SPJ_TESTDATA_DIR = "spj";
        public const string SPJ_DIRECTORY = "spj";

        private static Configuration _config = ConfigManager.Config;

        /// <summary>
        /// 获取评测目录下的SPJ目录
        /// </summary>
        /// <param name="task">评测任务</param>
        /// <returns>SPJ目录</returns>
        public static string GetSpjDirectoryInJudger(JudgeTask task)
        {
            if (task.TempJudgeDirectory.EndsWith(SPJ_DIRECTORY))
            {
                return task.TempJudgeDirectory;
            }
            return Path.Combine(task.TempJudgeDirectory, SPJ_DIRECTORY);
        }

        /// <summary>
        /// 获取测试数据目录下的SPJ目录
        /// </summary>
        /// <param name="problemId">题目ID</param>
        /// <returns>SPJ目录</returns>
        public static string GetSpjDirectoryInTestData(int problemId)
        {
            return Path.Combine(_config.TestDataDirectory, problemId.ToString(), SPJ_TESTDATA_DIR);
        }

        /// <summary>
        /// 获取评测目录下的SPJ程序的源文件路径
        /// </summary>
        /// <param name="task">评测任务</param>
        /// <returns>SPJ源文件名</returns>
        public static string GetSpjSourceFileInJudger(JudgeTask task)
        {
            return Path.Combine(GetSpjDirectoryInJudger(task), (task.LangConfig as ProgramLangConfig).SourceCodeFileName);
        }

        /// <summary>
        /// 寻找测试数据目录下的SPJ源文件
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="index">索引</param>
        /// <returns>SPJ源文件路径</returns>
        public static string FindSpjSourceFileInTestData(int problemId, int index = 0)
        {
            Dictionary<string, ProgramLangConfig> extDic = ConfigManager.GetLangSourceExtensionDictionary();
            string spjDirectory = GetSpjDirectoryInTestData(problemId);

            string[] files = Directory.GetFiles(spjDirectory);
            List<string> spjSourceFiles = new List<string>();
            foreach(var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                string fileExt = Path.GetExtension(file).TrimStart('.').ToLower();
                if (fileName == SPJ_SOURCE_FILENAME && extDic.ContainsKey(fileExt))
                {
                    spjSourceFiles.Add(PathHelper.GetBaseAbsolutePath(file));
                }
            }

            if (index < 0 || index >= spjSourceFiles.Count)
            {
                return null;
            }

            return spjSourceFiles[index];
        }

        /// <summary>
        /// 寻找评测目录下的SPJ程序
        /// </summary>
        /// <param name="task">评测任务</param>
        /// <returns>SPJ程序路径</returns>
        public static string FindSpjProgramInJudger(JudgeTask task)
        {
            string compileDirectory = GetSpjDirectoryInJudger(task);
            string[] files = Directory.GetFiles(compileDirectory);

            if (files.Length > 2)
            {
                LogManager.Warning("Can not confirm the unique special judge program!");
            }

            string spjProgramPath = null;
            foreach (string file in files)
            {
                if (Path.GetFileName(file) == (task.LangConfig as ProgramLangConfig).SourceCodeFileName)
                {
                    continue;
                }

                spjProgramPath = file;
            }

            return PathHelper.GetBaseAbsolutePath(spjProgramPath);
        }

        /// <summary>
        /// 寻找测试数据目录下的SPJ程序
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="index">索引</param>
        /// <returns>SPJ程序</returns>
        public static string FindSpjProgramInTestData(int problemId, int index = 0)
        {
            Dictionary<string, ProgramLangConfig> extDic = ConfigManager.GetLangSourceExtensionDictionary();
            string spjDirectory = GetSpjDirectoryInTestData(problemId);

            List<string> spjSourceFiles = new List<string>();
            string[] files = Directory.GetFiles(spjDirectory);
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file).ToLower();
                string fileExt = Path.GetExtension(fileName).TrimStart('.');
                fileName = Path.GetFileNameWithoutExtension(fileName);

                if (fileName == SPJ_PROGRAM_FILENAME && extDic.ContainsKey(fileExt))
                {
                    spjSourceFiles.Add(PathHelper.GetBaseAbsolutePath(file));
                }
            }

            if (index < 0 || index >= spjSourceFiles.Count)
            {
                return null;
            }

            return spjSourceFiles[index];
        }

        /// <summary>
        /// 获取评测目录下的SPJ程序路径
        /// </summary>
        /// <param name="task">评测任务</param>
        /// <returns>SPJ程序路径</returns>
        public static string GetSpjProgramPathInJudger(JudgeTask task)
        {
            string spjDirectory = GetSpjDirectoryInJudger(task);
            string path = Path.Combine(spjDirectory, (task.LangConfig as ProgramLangConfig).ProgramFileName);

            return PathHelper.GetBaseAbsolutePath(path);
        }

        /// <summary>
        /// 获取测试数据目录下的SPJ程序路径
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="lang">语言</param>
        /// <returns>SPJ程序路径</returns>
        public static string GetSpjProgramPathInTestData(int problemId, ProgramLangConfig lang)
        {
            string spjDirectory = GetSpjDirectoryInTestData(problemId);
            string programExt = Path.GetExtension(lang.ProgramFileName).TrimStart('.').ToLower();
            string sourceExt = lang.SourceCodeFileExtension.Split('|')[0].ToLower();
            string path = Path.Combine(spjDirectory, SPJ_PROGRAM_FILENAME + "." + sourceExt + "." + programExt);

            return PathHelper.GetBaseAbsolutePath(path);
        }

        /// <summary>
        /// 创建SPJ的JudgeTask, 用于编译运行SPJ程序
        /// </summary>
        /// <param name="originTask">源评测任务</param>
        /// <returns>SPJ的JudgeTask</returns>
        public static JudgeTask CreateSpjJudgeTask(JudgeTask originTask)
        {
            JudgeTask newTask = originTask.Clone() as JudgeTask;

            string spjSourceFilePath = FindSpjSourceFileInTestData(newTask.ProblemId);
            if (spjSourceFilePath == null) //没有SPJ程序源代码, 无法评测
            {
                throw new JudgeException("No special judge program exception!");
            }
            newTask.SourceCode = File.ReadAllText(spjSourceFilePath);

            newTask.LangConfig = GetLangConfigBySourceFilePath(spjSourceFilePath);
            ProgramLangConfig langConfig = newTask.LangConfig as ProgramLangConfig;
            newTask.Language = newTask.LangConfig.Name;

            string spjDir = GetSpjDirectoryInJudger(originTask) + "\\";

            // 替换<tempdir>字段
            langConfig.CompilerPath = langConfig.CompilerPath.Replace("<tempdir>", spjDir);
            langConfig.CompilerWorkDirectory = langConfig.CompilerWorkDirectory.Replace("<tempdir>", spjDir);
            langConfig.CompilerArgs = langConfig.CompilerArgs.Replace("<tempdir>", spjDir);
            langConfig.RunnerPath = langConfig.RunnerPath.Replace("<tempdir>", spjDir);
            langConfig.RunnerWorkDirectory = langConfig.RunnerWorkDirectory.Replace("<tempdir>", spjDir);
            langConfig.RunnerArgs = langConfig.RunnerArgs.Replace("<tempdir>", spjDir);

            // 使用绝对路径
            langConfig.CompilerPath = PathHelper.GetBaseAbsolutePath(langConfig.CompilerPath);
            langConfig.CompilerWorkDirectory = PathHelper.GetBaseAbsolutePath(langConfig.CompilerWorkDirectory);
            langConfig.RunnerWorkDirectory = PathHelper.GetBaseAbsolutePath(langConfig.RunnerWorkDirectory);
            langConfig.RunnerPath = PathHelper.GetBaseAbsolutePath(langConfig.RunnerPath);

            newTask.TempJudgeDirectory = spjDir;

            if (!Directory.Exists(spjDir)) 
            {
                Directory.CreateDirectory(spjDir);
            }

            return newTask;
        }

        /// <summary>
        /// 根据程序路径获取其关联的语言配置
        /// </summary>
        /// <param name="path">SPJ程序路径</param>
        /// <returns>语言配置</returns>
        public static ProgramLangConfig GetLangConfigByProgramPath(string path)
        {
            Dictionary<string, ProgramLangConfig> extDic = ConfigManager.GetLangSourceExtensionDictionary();

            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();
            string fileExt = Path.GetExtension(fileName).TrimStart('.');

            if (extDic.ContainsKey(fileExt))
            {
                return extDic[fileExt];
            }
            
            return null;
        }

        /// <summary>
        /// 根据SPJ源文件路径获取其关联的语言配置
        /// </summary>
        /// <param name="path">SPJ源文件路径</param>
        /// <returns>语言配置</returns>
        public static ProgramLangConfig GetLangConfigBySourceFilePath(string path)
        {
            Dictionary<string, ProgramLangConfig> extDic = ConfigManager.GetLangSourceExtensionDictionary();

            string fileExt = Path.GetExtension(path).TrimStart('.').ToLower();

            if (extDic.ContainsKey(fileExt))
            {
                return extDic[fileExt];
            }

            return null;
        }
    }
}
