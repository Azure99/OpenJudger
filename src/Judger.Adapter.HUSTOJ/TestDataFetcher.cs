using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Program;
using Judger.Utils;

namespace Judger.Adapter.HUSTOJ
{
    public class TestDataFetcher : BaseTestDataFetcher
    {
        public TestDataFetcher()
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
        }

        public override byte[] Fetch(JudgeContext context)
        {
            return Fetch(context.Task.ProblemId);
        }

        private byte[] Fetch(string problemId)
        {
            string[] fileNames = GetTestDataList(problemId);
            Tuple<string, byte[]>[] files = new Tuple<string, byte[]>[fileNames.Length];

            for (int i = 0; i < files.Length; i++)
            {
                string fileName = fileNames[i];
                files[i] = new Tuple<string, byte[]>(fileName, GetTestDataFile(problemId, fileName));
            }

            return CreateZip(files, GetTestDataMd5(problemId));
        }

        /// <summary>
        /// 根据题目ID获取测试数据文件名列表
        /// </summary>
        private string[] GetTestDataList(string pid)
        {
            string requestBody = "gettestdatalist=1&pid=" + pid;
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            string[] split = Regex.Split(response, "\r\n|\r|\n");

            return split
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
        }

        /// <summary>
        /// 根据题目ID和文件名获取测试数据
        /// </summary>
        /// <param name="pid">题目ID</param>
        /// <param name="fileName">远程文件名</param>
        private byte[] GetTestDataFile(string pid, string fileName)
        {
            string requestBody = "gettestdata=1&filename=" + pid + "/" + fileName;

            return HttpClient.UploadData(Config.TaskFetchUrl, requestBody, 3);
        }

        /// <summary>
        /// 将测试数据压缩为OpenJudger标准格式
        /// </summary>
        /// <param name="files">测试数据文件</param>
        /// <param name="dataVersion">测试数据版本号</param>
        private byte[] CreateZip(Tuple<string, byte[]>[] files, string dataVersion)
        {
            byte[] zipData;
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create))
                {
                    foreach (Tuple<string, byte[]> file in files)
                    {
                        ZipArchiveEntry entry = null;
                        if (file.Item1.EndsWith(".in"))
                            entry = zip.CreateEntry("input/" + file.Item1);
                        else if (file.Item1.EndsWith(".out"))
                            entry = zip.CreateEntry("output/" + file.Item1);
                        else if (CheckSpecialJudgeFile(file.Item1))
                        {
                            entry = zip.CreateEntry("spj/" + SpjManager.SpjSourceFilename +
                                                    Path.GetExtension(file.Item1));
                        }

                        if (entry != null)
                        {
                            using (Stream stream = entry.Open())
                                stream.Write(file.Item2);
                        }
                    }

                    ZipArchiveEntry verEntry = zip.CreateEntry("version.txt");
                    using (StreamWriter writer = new StreamWriter(verEntry.Open()))
                        writer.Write(dataVersion);

                    zip.UpdateBaseStream();

                    zipData = new byte[ms.Length];
                    int nowPos = (int) ms.Position;
                    ms.Position = 0;
                    ms.Read(zipData, 0, (int) ms.Length);
                    ms.Position = nowPos;
                }
            }

            return zipData;
        }

        /// <summary>
        /// 根据题目ID获取测试数据的MD5
        /// </summary>
        /// <param name="pid">题目ID</param>
        private string GetTestDataMd5(string pid)
        {
            string requestBody = $"gettestdatalist=1&pid={pid}&time=1";
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            return Md5Encrypt.EncryptToHexString(response);
        }

        /// <summary>
        /// 检查此文件是否为SPJ程序源代码文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns>是否为SPJ源代码</returns>
        private bool CheckSpecialJudgeFile(string fileName)
        {
            HashSet<string> extensionSet = new HashSet<string>();

            foreach (ProgramLangConfig lang in Config.Languages)
            {
                string[] extensions = lang.SourceCodeFileExtension.Split('|');
                foreach (string ext in extensions)
                {
                    if (!extensionSet.Contains(ext))
                        extensionSet.Add(ext);
                }
            }

            string name = Path.GetFileNameWithoutExtension(fileName).ToLower();
            string extension = Path.GetExtension(fileName).TrimStart('.').ToLower();

            if (name == "spj")
            {
                if (extensionSet.Contains(extension))
                    return true;
            }

            return false;
        }
    }
}