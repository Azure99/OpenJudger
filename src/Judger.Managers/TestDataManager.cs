using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Judger.Models;
using Judger.Models.Database;
using Judger.Models.Exception;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Managers
{
    /// <summary>
    /// 测试数据管理器
    /// </summary>
    public static class TestDataManager
    {
        /// <summary>
        /// 版本号文件名
        /// </summary>
        private const string VERSION_FILENAME = "version.txt";

        /// <summary>
        /// 测试输入文件夹名
        /// </summary>
        private const string DIR_INPUT = "input";

        /// <summary>
        /// 测试输出文件夹名
        /// </summary>
        private const string DIR_OUTPUT = "output";

        /// <summary>
        /// 测试查询文件夹名
        /// </summary>
        private const string DIR_QUERY = "query";

        /// <summary>
        /// 测试操作文件夹名
        /// </summary>
        private const string DIR_OPERATION = "oper";

        /// <summary>
        /// Special Judge数据文件夹名
        /// </summary>
        private const string DIR_SPJ = "spj";

        /// <summary>
        /// Database Judge数据文件夹名
        /// </summary>
        private const string DIR_DB = "db";

        /// <summary>
        /// 数据锁字典, 防止统一题目测试数据争用
        /// </summary>
        private static readonly Dictionary<int, object> _dataLockDic;

        /// <summary>
        /// 数据锁字典的锁
        /// </summary>
        private static readonly object _dicLock = new object();

        static TestDataManager()
        {
            _dataLockDic = new Dictionary<int, object>();

            if (!Directory.Exists(Config.TestDataDirectory))
                Directory.CreateDirectory(Config.TestDataDirectory);
        }

        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 检查本地测试数据版本是否与最新版本一致, 如果不是就需要重新获取数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="version">最新版本</param>
        public static bool CheckData(int problemId, string version)
        {
            string dirPath = Cmb(Config.TestDataDirectory, problemId);
            string verPath = Cmb(dirPath, VERSION_FILENAME);

            lock (GetDataLock(problemId))
            {
                if (!Directory.Exists(dirPath) || !File.Exists(verPath))
                    return false;

                FileHelper.TryReadAllText(verPath, out string localVersion);
                return localVersion == version;
            }
        }

        /// <summary>
        /// 写出测试数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="zipData">ZIP数据</param>
        public static void WriteTestData(int problemId, byte[] zipData)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(zipData);
                ms.Position = 0;
                WriteTestData(problemId, ms);
            }
        }

        /// <summary>
        /// 写出测试数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="zipStream">保存ZIP的Stream</param>
        public static void WriteTestData(int problemId, Stream zipStream)
        {
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                WriteTestData(problemId, zipArchive);
        }

        /// <summary>
        /// 写出测试数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="zipArchive">ZipArchive</param>
        public static void WriteTestData(int problemId, ZipArchive zipArchive)
        {
            string dirPath = Cmb(Config.TestDataDirectory, problemId);
            lock (GetDataLock(problemId))
            {
                if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);

                zipArchive.ExtractToDirectory(dirPath);
            }
        }

        /// <summary>
        /// 获取测试数据文件名列表, 使用GetTestData方法获取具体数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <returns>测试数据(输入/输出)文件名</returns>
        public static ProgramTestDataFile[] GetTestDataFilesName(int problemId)
        {
            string dirPath = Cmb(Config.TestDataDirectory, problemId);

            string[] inputFiles;
            string[] outputFiles;
            lock (GetDataLock(problemId))
            {
                inputFiles = Directory.GetFiles(Cmb(dirPath, DIR_INPUT));
                for (var i = 0; i < inputFiles.Length; i++)
                    inputFiles[i] = Path.GetFileName(inputFiles[i]);

                outputFiles = Directory.GetFiles(Cmb(dirPath, DIR_OUTPUT));
                for (var i = 0; i < outputFiles.Length; i++)
                    outputFiles[i] = Path.GetFileName(outputFiles[i]);
            }

            var query = from input in inputFiles
                from output in outputFiles
                where Path.GetFileNameWithoutExtension(input) ==
                      Path.GetFileNameWithoutExtension(output)
                select new ProgramTestDataFile
                {
                    Name = Path.GetFileNameWithoutExtension(input),
                    InputFile = input,
                    OutputFile = output
                };

            return query.ToArray();
        }

        /// <summary>
        /// 获取指定的测试数据
        /// </summary>
        /// <param name="problemId">题目ID</param>
        /// <param name="dataFile">程序测试数据文件</param>
        /// <returns>测试数据</returns>
        public static ProgramTestData GetTestData(int problemId, ProgramTestDataFile dataFile)
        {
            string inputName = dataFile.InputFile;
            string outputName = dataFile.OutputFile;
            string dirPath = Cmb(Config.TestDataDirectory, problemId);

            lock (GetDataLock(problemId))
            {
                return new ProgramTestData
                {
                    Name = Path.GetFileNameWithoutExtension(inputName),
                    Input =  File.ReadAllText(Cmb(dirPath, DIR_INPUT, inputName)),
                    Output =  File.ReadAllText(Cmb(dirPath, DIR_OUTPUT, outputName))
                };
            }
        }

        /// <summary>
        /// 检查题目是否需要SPJ
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <returns>是否需要SPJ</returns>
        public static bool IsSpecialJudge(int problemId)
        {
            string spjDir = Cmb(Config.TestDataDirectory, problemId, DIR_SPJ);

            lock (GetDataLock(problemId))
            {
                return Directory.Exists(spjDir) && Directory.GetFiles(spjDir).Length > 0;
            }
        }

        /// <summary>
        /// 写出SPJ可执行程序
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="programFile">SPJ程序</param>
        public static void WriteSpecialJudgeProgramFile(int problemId, SpecialJudgeProgram programFile)
        {
            lock (GetDataLock(problemId))
            {
                string programPath = SpjManager.GetSpjProgramPathInTestData(problemId, programFile.LangConfiguration);
                if (File.Exists(programPath))
                    File.Delete(programPath);

                File.WriteAllBytes(programPath, programFile.Program);
            }
        }

        /// <summary>
        /// 获取SPJ可执行程序
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="index">索引</param>
        /// <returns>SPJ程序</returns>
        public static SpecialJudgeProgram GetSpecialJudgeProgramFile(int problemId, int index = 0)
        {
            lock (GetDataLock(problemId))
            {
                string programPath = SpjManager.FindSpjProgramInTestData(problemId, index);
                if (programPath == null)
                    return null;

                ProgramLangConfig langConfig = SpjManager.GetLangConfigByProgramPath(programPath);
                if (langConfig == null)
                    return null;

                return new SpecialJudgeProgram
                {
                    LangConfiguration = langConfig,
                    Program = File.ReadAllBytes(programPath)
                };
            }
        }

        /// <summary>
        /// 根据题目ID获取全部数据库测试数据的名称
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="dbType">数据库名称</param>
        /// <returns>全部测试数据的名称</returns>
        public static string[] GetDbTestDataNames(int problemId, DatabaseType dbType)
        {
            lock (GetDataLock(problemId))
            {
                string dirPath = Cmb(Config.TestDataDirectory, problemId, DIR_DB);

                string[] inputFiles = Directory.GetFiles(Cmb(dirPath, DIR_INPUT));
                IEnumerable<string> query = from x in inputFiles
                    where Path.GetExtension(x).TrimStart('.').ToLower() == dbType.ToString().ToLower()
                    select Path.GetFileNameWithoutExtension(x);

                return query.ToArray();
            }
        }

        /// <summary>
        /// 根据问题ID和数据名称获取数据库测试数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="dataName">测试数据名称</param>
        /// <returns>测试数据</returns>
        public static DbTestData GetDbTestData(int problemId, DatabaseType dbType, string dataName)
        {
            lock (GetDataLock(problemId))
            {
                string dirPath = Cmb(Config.TestDataDirectory, problemId, DIR_DB);

                string inputFile = Cmb(dirPath, DIR_INPUT, dataName + '.' + dbType);

                IEnumerable<string> operFileQuery = from x in Directory.GetFiles(Cmb(dirPath, DIR_OPERATION))
                    where Path.GetFileNameWithoutExtension(x).ToLower() == dbType.ToString().ToLower()
                    select x;

                IEnumerable<string> queryFileQuery = from x in Directory.GetFiles(Cmb(dirPath, DIR_QUERY))
                    where Path.GetFileNameWithoutExtension(x).ToLower() == dbType.ToString().ToLower()
                    select x;

                string operFile = operFileQuery.FirstOrDefault();
                string queryFile = queryFileQuery.FirstOrDefault();

                if (!File.Exists(inputFile))
                    throw new JudgeException("Database input file not found: " + inputFile);

                if (operFile == null && queryFile == null)
                    throw new JudgeException("Database output file not found: " + inputFile);

                return new DbTestData
                {
                    Name = dataName,
                    Input = File.ReadAllText(inputFile),
                    Operation = operFile != null ? File.ReadAllText(operFile) : null,
                    Query = queryFile != null ? File.ReadAllText(queryFile) : null
                };
            }
        }

        /// <summary>
        /// 检查题目是否为数据库题目
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <returns>是否为数据库题目</returns>
        public static bool IsDatabaseJudge(int problemId)
        {
            string dbDir = Cmb(Config.TestDataDirectory, problemId, DIR_DB);

            lock (GetDataLock(problemId))
            {
                if (Directory.Exists(dbDir) && Directory.GetDirectories(dbDir).Length > 0)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// 获取测试数据锁, 保证每个题目的数据只能同时由一个JudgeTask访问
        /// </summary>
        /// <param name="problemId">题目ID</param>
        /// <returns>锁</returns>
        private static object GetDataLock(int problemId)
        {
            lock (_dicLock)
            {
                if (!_dataLockDic.ContainsKey(problemId))
                    _dataLockDic.Add(problemId, new object());

                return _dataLockDic[problemId];
            }
        }

        /// <summary>
        /// 合并路径, Path.Combine方法缩写
        /// </summary>
        /// <param name="paths">欲合并路径</param>
        /// <returns>合并后的路径</returns>
        private static string Cmb(params object[] paths)
        {
            var pathStrings = new string[paths.Length];
            for (var i = 0; i < pathStrings.Length; i++)
                pathStrings[i] = paths[i].ToString();

            return Path.Combine(pathStrings);
        }
    }
}