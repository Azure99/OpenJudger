using System.Collections.Concurrent;
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
        private const string ConstVersionFilename = "version.txt";
        private const string ConstDirInput = "input";
        private const string ConstDirOutput = "output";
        private const string ConstDirQuery = "query";
        private const string ConstDirOperation = "oper";
        private const string ConstDirSpj = "spj";
        private const string ConstDirDb = "db";

        // 数据锁字典, 防止统一题目测试数据争用
        private static readonly ConcurrentDictionary<string, object> DataLockDic;

        static TestDataManager()
        {
            DataLockDic = new ConcurrentDictionary<string, object>();

            if (!Directory.Exists(Config.TestDataDirectory))
                Directory.CreateDirectory(Config.TestDataDirectory);
        }

        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 检查本地测试数据版本是否为目标版本
        /// </summary>
        public static bool CheckDataVersion(string problemId, string version)
        {
            string dirPath = Cmb(Config.TestDataDirectory, problemId);
            string verPath = Cmb(dirPath, ConstVersionFilename);

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
        public static void WriteTestData(string problemId, byte[] zipData)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(zipData);
                stream.Position = 0;
                using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    string dirPath = Cmb(Config.TestDataDirectory, problemId);
                    lock (GetDataLock(problemId))
                    {
                        if (Directory.Exists(dirPath))
                            Directory.Delete(dirPath, true);

                        zipArchive.ExtractToDirectory(dirPath);
                    }
                }
            }
        }

        /// <summary>
        /// 获取测试数据文件列表
        /// </summary>
        /// 请使用GetTestData方法获取具体数据
        public static ProgramTestDataFile[] GetTestDataFiles(string problemId)
        {
            string dirPath = Cmb(Config.TestDataDirectory, problemId);

            string[] inputFiles;
            string[] outputFiles;
            lock (GetDataLock(problemId))
            {
                inputFiles = Directory.GetFiles(Cmb(dirPath, ConstDirInput));
                for (int i = 0; i < inputFiles.Length; i++)
                    inputFiles[i] = Path.GetFileName(inputFiles[i]);

                outputFiles = Directory.GetFiles(Cmb(dirPath, ConstDirOutput));
                for (int i = 0; i < outputFiles.Length; i++)
                    outputFiles[i] = Path.GetFileName(outputFiles[i]);
            }

            ProgramTestDataFile[] files = (
                from input in inputFiles
                from output in outputFiles
                where Path.GetFileNameWithoutExtension(input) ==
                      Path.GetFileNameWithoutExtension(output)
                select new ProgramTestDataFile
                {
                    Name = Path.GetFileNameWithoutExtension(input),
                    InputFile = input,
                    OutputFile = output
                }).ToArray();

            return files;
        }

        /// <summary>
        /// 获取指定的测试数据
        /// </summary>
        public static ProgramTestData GetTestData(string problemId, ProgramTestDataFile dataFile)
        {
            string inputName = dataFile.InputFile;
            string outputName = dataFile.OutputFile;
            string dirPath = Cmb(Config.TestDataDirectory, problemId);

            lock (GetDataLock(problemId))
            {
                return new ProgramTestData
                {
                    Name = Path.GetFileNameWithoutExtension(inputName),
                    Input = File.ReadAllText(Cmb(dirPath, ConstDirInput, inputName)),
                    Output = File.ReadAllText(Cmb(dirPath, ConstDirOutput, outputName))
                };
            }
        }

        /// <summary>
        /// 检查题目是否需要Special Judge
        /// </summary>
        public static bool CheckNeedSpecialJudge(string problemId)
        {
            string spjDir = Cmb(Config.TestDataDirectory, problemId, ConstDirSpj);

            lock (GetDataLock(problemId))
            {
                return Directory.Exists(spjDir) && Directory.GetFiles(spjDir).Length > 0;
            }
        }

        /// <summary>
        /// 写出SPJ可执行程序
        /// </summary>
        public static void WriteSpecialJudgeProgramFile(string problemId, SpecialJudgeProgram programFile)
        {
            lock (GetDataLock(problemId))
            {
                string programPath = SpjManager.GetSpjProgramPathInTestData(problemId, programFile.LangConfig);
                if (File.Exists(programPath))
                    File.Delete(programPath);

                File.WriteAllBytes(programPath, programFile.Program);
            }
        }

        /// <summary>
        /// 获取SPJ可执行程序
        /// </summary>
        public static SpecialJudgeProgram GetSpecialJudgeProgramFile(string problemId, int index = 0)
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
                    LangConfig = langConfig,
                    Program = File.ReadAllBytes(programPath)
                };
            }
        }

        /// <summary>
        /// 根据题目Id获取全部数据库测试数据的名称
        /// </summary>
        public static string[] GetDbTestDataNames(string problemId, DatabaseType dbType)
        {
            lock (GetDataLock(problemId))
            {
                string dirPath = Cmb(Config.TestDataDirectory, problemId, ConstDirDb);

                string[] inputFiles = Directory.GetFiles(Cmb(dirPath, ConstDirInput));

                string[] dataNames = (
                    from x in inputFiles
                    where Path.GetExtension(x).TrimStart('.').EqualsIgnoreCase(dbType.ToString())
                    select Path.GetFileNameWithoutExtension(x)
                ).ToArray();

                return dataNames;
            }
        }

        /// <summary>
        /// 根据问题Id和数据名称获取数据库测试数据
        /// </summary>
        public static DbTestData GetDbTestData(string problemId, DatabaseType dbType, string dataName)
        {
            lock (GetDataLock(problemId))
            {
                string dirPath = Cmb(Config.TestDataDirectory, problemId, ConstDirDb);

                string inputFile = Cmb(dirPath, ConstDirInput, dataName + '.' + dbType);

                string operFile = (
                    from x in Directory.GetFiles(Cmb(dirPath, ConstDirOperation))
                    where Path.GetFileNameWithoutExtension(x).EqualsIgnoreCase(dbType.ToString())
                    select x
                ).FirstOrDefault();

                string queryFile = (
                    from x in Directory.GetFiles(Cmb(dirPath, ConstDirQuery))
                    where Path.GetFileNameWithoutExtension(x).EqualsIgnoreCase(dbType.ToString())
                    select x
                ).FirstOrDefault();

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
        public static bool CheckIsDatabaseJudge(string problemId)
        {
            string dbDir = Cmb(Config.TestDataDirectory, problemId, ConstDirDb);

            lock (GetDataLock(problemId))
            {
                return Directory.Exists(dbDir) && Directory.GetDirectories(dbDir).Length > 0;
            }
        }

        /// <summary>
        /// 获取测试数据锁
        /// </summary>
        /// 保证每个题目的数据只能同时由一个JudgeTask访问
        private static object GetDataLock(string problemId)
        {
            if (!DataLockDic.ContainsKey(problemId))
                DataLockDic.TryAdd(problemId, new object());

            return DataLockDic[problemId];
        }

        /// <summary>
        /// 合并路径
        /// </summary>
        /// Path.Combine方法缩写
        private static string Cmb(params object[] paths)
        {
            string[] pathStrings = new string[paths.Length];
            for (int i = 0; i < pathStrings.Length; i++)
                pathStrings[i] = paths[i].ToString();

            return Path.Combine(pathStrings);
        }
    }
}