using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Judger.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher.HUSTOJ
{
    public class TestDataFetcher : BaseTestDataFetcher
    {
        public TestDataFetcher()
        {
            HttpClient.CookieContainer = Authenticator.Singleton.CookieContainer;
        }

        public override byte[] Fetch(JudgeTask task)
        {
            return Fetch(task.ProblemID);
        }

        public byte[] Fetch(int problemID)
        {
            string[] fileNames = GetTestDataList(problemID);
            Tuple<string, byte[]>[] files = new Tuple<string, byte[]>[fileNames.Length];

            for (int i = 0; i < files.Length; i++)
            {
                string fileName = fileNames[i];
                files[i] = new Tuple<string, byte[]>(fileName, GetTestDataFile(problemID, fileName));
            }

            return CreateZIP(files, GetTestDataMD5(problemID));
        }

        /// <summary>
        /// 根据题目ID获取测试数据文件名列表
        /// </summary>
        /// <param name="pid">题目ID</param>
        private string[] GetTestDataList(int pid)
        {
            string requestBody = "gettestdatalist=1&pid=" + pid;
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            string[] split = Regex.Split(response, "\r\n|\r|\n");

            List<string> dataList = new List<string>();
            foreach (string s in split)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    dataList.Add(s);
                }
            }

            return dataList.ToArray();
        }

        /// <summary>
        /// 根据题目ID和文件名获取测试数据
        /// </summary>
        /// <param name="pid">题目ID</param>
        /// <param name="fileName">远程文件名</param>
        private byte[] GetTestDataFile(int pid, string fileName)
        {
            string requestBody = "gettestdata=1&filename=" + pid + "/" + fileName;

            return HttpClient.UploadData(Config.TaskFetchUrl, requestBody, 3);
        }

        /// <summary>
        /// 将测试数据压缩为OpenJudger标准格式
        /// </summary>
        /// <param name="files">测试数据文件</param>
        /// <param name="dataVersion">测试数据版本号</param>
        private byte[] CreateZIP(Tuple<string, byte[]>[] files, string dataVersion)
        {
            byte[] zipData;
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create))
                {
                    foreach(var file in files)
                    {
                        ZipArchiveEntry entry = null;
                        if (file.Item1.EndsWith(".in"))
                        {
                            entry = zip.CreateEntry("input/" + file.Item1);
                        }
                        else if (file.Item1.EndsWith(".out"))
                        {
                            entry = zip.CreateEntry("output/" + file.Item1);
                        }
                        else if (CheckSpecialJudgeFile(file.Item1))
                        {
                            entry = zip.CreateEntry("spj/" + SPJManager.SPJ_SOURCE_FILENAME + Path.GetExtension(file.Item1));
                        }

                        if (entry != null)
                        {
                            using (Stream stream = entry.Open())
                            {
                                stream.Write(file.Item2);
                            }
                        }
                    }

                    ZipArchiveEntry verEntry = zip.CreateEntry("version.txt");
                    using (StreamWriter sw = new StreamWriter(verEntry.Open()))
                    {
                        sw.Write(dataVersion);
                    }

                    zip.UpdateBaseStream();
                    
                    zipData = new byte[ms.Length];
                    int nowPos = (int)ms.Position;
                    ms.Position = 0;
                    ms.Read(zipData, 0, (int)ms.Length);
                    ms.Position = nowPos;
                }
            }

            return zipData;
        }

        /// <summary>
        /// 根据题目ID获取测试数据的MD5
        /// </summary>
        /// <param name="pid">题目ID</param>
        private string GetTestDataMD5(int pid)
        {
            string requestBody = string.Format("gettestdatalist=1&pid={0}&time=1", pid);
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            return MD5Encrypt.EncryptToHexString(response);
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
                    {
                        extensionSet.Add(ext);
                    }
                }
            }

            string name = Path.GetFileNameWithoutExtension(fileName).ToLower();
            string extension = Path.GetExtension(fileName).TrimStart('.').ToLower();

            if(name == "spj")
            {
                if(extensionSet.Contains(extension))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
