using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using Judger.Fetcher;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.HUSTOJ
{
    public class TestDataFetcher : ITestDataFetcher
    {
        //最大测试数据下载时间
        private const int MAX_DOWNLOAD_TIME = 600000;
        private readonly Configuration _config = ConfigManager.Config;
        private HttpWebClient _webClient = ConfiguredClient.Create(); 
        public TestDataFetcher()
        {
            _webClient.ReadWriteTimeout = MAX_DOWNLOAD_TIME;
            _webClient.CookieContainer = Authenticator.Singleton.CookieContainer;
        }

        public byte[] Fetch(int problemID)
        {
            string[] fileNameArr = GetTestDataList(problemID);
            Tuple<string, byte[]>[] files = new Tuple<string, byte[]>[fileNameArr.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string fileName = fileNameArr[i];
                files[i] = new Tuple<string, byte[]>(fileName, GetTestDataFile(problemID, fileName));
            }

            return CreateZIP(files, GetTestDataMD5(problemID));
        }

        private string[] GetTestDataList(int pid)
        {
            string body = "gettestdatalist=1&pid=" + pid;
            string res = _webClient.UploadString(_config.TaskFetchUrl, body, 3);

            string[] split = Regex.Split(res, "\r\n|\r|\n");

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

        private byte[] GetTestDataFile(int pid, string fileName)
        {
            string body = "gettestdata=1&filename=" + pid + "/" + fileName;
            return _webClient.UploadData(_config.TaskFetchUrl, body, 3);
        }

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
                        if(file.Item1.EndsWith(".in"))
                        {
                            entry = zip.CreateEntry("input/" + file.Item1);
                        }
                        else if(file.Item1.EndsWith(".out"))
                        {
                            entry = zip.CreateEntry("output/" + file.Item1);
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

        private string GetTestDataMD5(int pid)
        {
            string body = string.Format("gettestdatalist=1&pid={0}&time=1", pid);
            string res = _webClient.UploadString(_config.TaskFetchUrl, body, 3);

            return MD5Encrypt.EncryptToHexString(res);
        }

        public byte[] Fetch(string problemID)
        {
            return Fetch(int.Parse(problemID));
        }
        public byte[] Fetch(JudgeTask task)
        {
            return Fetch(task.ProblemID);
        }

        public void Dispose()
        {
            _webClient.Dispose();
        }
    }
}
