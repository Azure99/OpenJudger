using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.IO.Compression;
using Judger.Utils;
using Judger.Models;

namespace Judger.Managers
{
    /// <summary>
    /// 测试数据管理器
    /// </summary>
    public static class TestDataManager
    {
        private static Configuration _config = ConfigManager.Config;

        //数据锁字典, 防止统一题目测试数据争用
        private static Dictionary<int, object> _dataLockDic;

        //数据锁字典的锁
        private static object _dicLock = new object();

        /// <summary>
        /// 目录分隔符
        /// </summary>
        private static readonly char SepChar = Path.DirectorySeparatorChar;

        static TestDataManager()
        {
            _dataLockDic = new Dictionary<int, object>();

            if(!Directory.Exists(_config.TestDataDirectory))
            {
                Directory.CreateDirectory(_config.TestDataDirectory);
            }
        }

        /// <summary>
        /// 检查测试数据版本是否正确, 若不正确返回false, 需要重新获取数据
        /// </summary>
        /// <param name="problemID">问题ID</param>
        /// <param name="version">欲检测版本</param>
        public static bool CheckData(int problemID, string version)
        {
            string path = Path.Combine(_config.TestDataDirectory, problemID.ToString());

            lock(GetDataLock(problemID))
            {
                if (!Directory.Exists(path) || 
                    !Directory.Exists(path + SepChar + "input") || 
                    !Directory.Exists(path + SepChar + "output") ||
                    !File.Exists(path + SepChar + "version.txt"))
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
        /// <param name="problemID">问题ID</param>
        /// <returns>版本号</returns>
        public static string GetTestDataVersion(int problemID)
        {
            string path = Path.Combine(_config.TestDataDirectory, 
                                       problemID.ToString(), 
                                       "version.txt");


            lock (GetDataLock(problemID))
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
        /// <param name="problemID">问题ID</param>
        /// <param name="zipStream">保存ZIP的Stream</param>
        public static void WriteTestData(int problemID, Stream zipStream)
        {
            using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                WriteTestData(problemID, zipArchive);
            }
        }

        /// <summary>
        /// 写出测试数据
        /// </summary>
        /// <param name="problemID">问题ID</param>
        /// <param name="zipArchive">ZipArchive</param>
        public static void WriteTestData(int problemID, ZipArchive zipArchive)
        {
            string path = Path.Combine(_config.TestDataDirectory, problemID.ToString());
            lock (GetDataLock(problemID))
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
        /// <param name="problemID">问题ID</param>
        /// <param name="zipData">ZIP数据</param>
        public static void WriteTestData(int problemID, byte[] zipData)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(zipData);
                ms.Position = 0;
                WriteTestData(problemID, ms);
            }
        }

        /// <summary>
        /// 获取测试数据文件名列表, 使用GetTestData方法获取具体数据
        /// </summary>
        /// <param name="problemID">问题ID</param>
        /// <returns>测试数据(输入/输出)文件名，使用元组数组保存</returns>
        public static Tuple<string, string>[] GetTestDataFilesName(int problemID)
        {
            string path = Path.Combine(_config.TestDataDirectory, problemID.ToString()) + SepChar;

            string[] inputFiles;
            string[] outputFiles;
            lock (GetDataLock(problemID))
            {
                inputFiles = Directory.GetFiles(path + "input");
                for (int i = 0; i < inputFiles.Length; i++)
                {
                    inputFiles[i] = Path.GetFileName(inputFiles[i]);
                }

                outputFiles = Directory.GetFiles(path + "output");
                for(int i=0; i<outputFiles.Length; i++)
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
            foreach(var testData in query)
            {
                matchedFiles.Add(new Tuple<string, string>(testData.InputFile, testData.OutputFile));
            }

            return matchedFiles.ToArray();
        }

        /// <summary>
        /// 获取指定的测试数据
        /// </summary>
        /// <param name="problemID">题目ID</param>
        /// <param name="inputName">测试输入名称</param>
        /// <param name="outputName">测试输出名称</param>
        /// <param name="input">测试输入</param>
        /// <param name="output">测试输出</param>
        public static void GetTestData(int problemID, string inputName, string outputName, out string input, out string output)
        {
            string problemDir = Path.Combine(_config.TestDataDirectory, problemID.ToString()) + SepChar;

            lock(GetDataLock(problemID))
            {
                input = File.ReadAllText(problemDir + "input" + SepChar + inputName);
                output = File.ReadAllText(problemDir + "output" + SepChar + outputName);
            }
        }

        /// <summary>
        /// 获取测试数据锁, 保证每个题目的数据只能同时由一个JudgeTask访问
        /// </summary>
        /// <param name="problemID">题目ID</param>
        /// <returns>锁</returns>
        private static object GetDataLock(int problemID)
        {
            lock(_dicLock)
            {
                if (!_dataLockDic.ContainsKey(problemID))
                {
                    _dataLockDic.Add(problemID, new object());
                }

                return _dataLockDic[problemID];
            }
        }
    }
}
