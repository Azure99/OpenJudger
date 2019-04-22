using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Judger.Entity;
using Judger.Entity.Database;
using Judger.Entity.Program;
using Judger.Utils;

namespace Judger.Managers
{
    /// <summary>
    /// 测试数据管理器
    /// </summary>
    public static class TestDataManager
    {
        private static readonly Configuration Config = ConfigManager.Config;

        /// <summary>
        /// 数据锁字典, 防止统一题目测试数据争用
        /// </summary>
        private static Dictionary<int, object> _dataLockDic;

        /// <summary>
        /// 数据锁字典的锁
        /// </summary>
        private static object _dicLock = new object();

        /// <summary>
        /// 目录分隔符
        /// </summary>
        private static readonly char SepChar = Path.DirectorySeparatorChar;

        static TestDataManager()
        {
            _dataLockDic = new Dictionary<int, object>();

            if (!Directory.Exists(Config.TestDataDirectory))
            {
                Directory.CreateDirectory(Config.TestDataDirectory);
            }
        }

        /// <summary>
        /// 检查测试数据版本是否正确, 若不正确返回false, 需要重新获取数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="version">欲检测版本</param>
        public static bool CheckData(int problemId, string version)
        {
            string path = Path.Combine(Config.TestDataDirectory, problemId.ToString());

            lock (GetDataLock(problemId))
            {
                if (!Directory.Exists(path) || !File.Exists(path + SepChar + "version.txt"))
                {
                    return false;
                }

                FileHelper.TryReadAllText(path + SepChar + "version.txt", out string localVersion);
                return localVersion == version;
            }
        }

        /// <summary>
        /// 获取当前测试数据版本号
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <returns>版本号</returns>
        public static string GetTestDataVersion(int problemId)
        {
            string path = Path.Combine(Config.TestDataDirectory,
                                       problemId.ToString(),
                                       "version.txt");


            lock (GetDataLock(problemId))
            {
                if (!File.Exists(path))
                {
                    return "";
                }

                FileHelper.TryReadAllText(path, out string version);
                return version;
            }
        }

        /// <summary>
        /// 写出测试数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="zipStream">保存ZIP的Stream</param>
        public static void WriteTestData(int problemId, Stream zipStream)
        {
            using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                WriteTestData(problemId, zipArchive);
            }
        }

        /// <summary>
        /// 写出测试数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="zipArchive">ZipArchive</param>
        public static void WriteTestData(int problemId, ZipArchive zipArchive)
        {
            string path = Path.Combine(Config.TestDataDirectory, problemId.ToString());
            lock (GetDataLock(problemId))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch { }

                zipArchive.ExtractToDirectory(path);
            }
        }

        /// <summary>
        /// 写出测试数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <param name="zipData">ZIP数据</param>
        public static void WriteTestData(int problemId, byte[] zipData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(zipData);
                ms.Position = 0;
                WriteTestData(problemId, ms);
            }
        }

        /// <summary>
        /// 获取测试数据文件名列表, 使用GetTestData方法获取具体数据
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <returns>测试数据(输入/输出)文件名，使用元组数组保存</returns>
        public static Tuple<string, string>[] GetTestDataFilesName(int problemId)
        {
            string path = Path.Combine(Config.TestDataDirectory, problemId.ToString()) + SepChar;

            string[] inputFiles;
            string[] outputFiles;
            lock (GetDataLock(problemId))
            {
                inputFiles = Directory.GetFiles(path + "input");
                for (int i = 0; i < inputFiles.Length; i++)
                {
                    inputFiles[i] = Path.GetFileName(inputFiles[i]);
                }

                outputFiles = Directory.GetFiles(path + "output");
                for (int i = 0; i < outputFiles.Length; i++)
                {
                    outputFiles[i] = Path.GetFileName(outputFiles[i]);
                }
            }

            var query = from input in inputFiles
                        from output in outputFiles
                        where Path.GetFileNameWithoutExtension(input) ==
                              Path.GetFileNameWithoutExtension(output)
                        select new
                        {
                            InputFile = input,
                            OutputFile = output
                        };

            List<Tuple<string, string>> matchedFiles = new List<Tuple<string, string>>();
            foreach (var testData in query)
            {
                matchedFiles.Add(new Tuple<string, string>(testData.InputFile, testData.OutputFile));
            }

            return matchedFiles.ToArray();
        }

        /// <summary>
        /// 获取指定的测试数据
        /// </summary>
        /// <param name="problemId">题目ID</param>
        /// <param name="inputName">测试输入名称</param>
        /// <param name="outputName">测试输出名称</param>
        /// <param name="input">测试输入</param>
        /// <param name="output">测试输出</param>
        public static void GetTestData(int problemId, string inputName, string outputName, out string input, out string output)
        {
            string problemDir = Path.Combine(Config.TestDataDirectory, problemId.ToString()) + SepChar;

            lock (GetDataLock(problemId))
            {
                input = File.ReadAllText(problemDir + "input" + SepChar + inputName);
                output = File.ReadAllText(problemDir + "output" + SepChar + outputName);
            }
        }

        /// <summary>
        /// 检查题目是否需要SPJ
        /// </summary>
        /// <param name="problemId">问题ID</param>
        /// <returns>是否需要SPJ</returns>
        public static bool IsSpecialJudge(int problemId)
        {
            string spjDir = Path.Combine(Config.TestDataDirectory, problemId.ToString(), "spj") + SepChar;

            lock (GetDataLock(problemId))
            {
                if (Directory.Exists(spjDir))
                {
                    if (Directory.GetFiles(spjDir).Length > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [Obsolete]
        public static SpecialJudgeSourceFile GetSpecialJudgeSourceFile(int problemId, int index = 0)
        {
            string sourceFilePath = SpjManager.FindSpjSourceFileInTestData(problemId, index);
            if (sourceFilePath == null)
            {
                return null;
            }

            ProgramLangConfig langConfig = SpjManager.GetLangConfigBySourceFilePath(sourceFilePath);
            if (langConfig == null)
            {
                return null;
            }

            return new SpecialJudgeSourceFile
            {
                LangConfiguration = langConfig,
                SourceCode = File.ReadAllText(sourceFilePath)
            };
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
                {
                    File.Delete(programPath);
                }
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
                {
                    return null;
                }

                ProgramLangConfig langConfig = SpjManager.GetLangConfigByProgramPath(programPath);
                if (langConfig == null)
                {
                    return null;
                }

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
                string path = Path.Combine(Config.TestDataDirectory, problemId.ToString()) + SepChar + "db" + SepChar;

                string[] inputFiles = Directory.GetFiles(path + "input");
                var query = from x in inputFiles
                            where Path.GetExtension(x).TrimStart('.').ToLower() == dbType.ToString().ToLower()
                            select Path.GetFileNameWithoutExtension(x);

                List<string> dataNames = new List<string>();
                foreach (var x in query)
                {
                    string outputFile = PathHelper.FindFileIgnoreCase(path + "output", x + "." + dbType);
                    string queryFile = PathHelper.FindFileIgnoreCase(path + "query", x + "." + dbType);
                    if (outputFile != null || queryFile != null)
                    {
                        dataNames.Add(x);
                    }
                }

                return dataNames.ToArray();
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
                string path = Path.Combine(Config.TestDataDirectory, problemId.ToString()) + SepChar + "db" + SepChar;

                string inputFile = PathHelper.FindFileIgnoreCase(path + "input", dataName + '.' + dbType);
                string outputFile = PathHelper.FindFileIgnoreCase(path + "output", dataName + "." + dbType);
                string queryFile = PathHelper.FindFileIgnoreCase(path + "query", dataName + "." + dbType);

                if (inputFile == null || (outputFile == null && queryFile == null))
                {
                    throw new JudgeException("Database input file not found: " + inputFile);
                }

                return new DbTestData
                {
                    Name = dataName,
                    Input = (inputFile != null) ? File.ReadAllText(inputFile) : null,
                    Output = (outputFile != null) ? File.ReadAllText(outputFile) : null,
                    Query = (queryFile != null) ? File.ReadAllText(queryFile) : null,
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
            string dbDir = Path.Combine(Config.TestDataDirectory, problemId.ToString(), "db") + SepChar;

            lock (GetDataLock(problemId))
            {
                if (Directory.Exists(dbDir))
                {
                    if (Directory.GetDirectories(dbDir).Length > 0)
                    {
                        return true;
                    }
                }
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
                {
                    _dataLockDic.Add(problemId, new object());
                }

                return _dataLockDic[problemId];
            }
        }
    }
}
