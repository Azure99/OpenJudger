using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// TestData获取器
    /// </summary>
    public class TestDataFetcher : ITestDataFetcher
    {
        //最大测试数据下载时间
        private const int MAX_DOWNLOAD_TIME = 600000;
        private readonly Configuration _config = ConfigManager.Config;
        private HttpWebClient _httpClient = ConfiguredClient.Create();

        public TestDataFetcher()
        {
            _httpClient.CookieContainer = Authenticator.Singleton.CookieContainer;
        }

        public byte[] Fetch(string problemID)
        {
            string body = CreateRequestBody(problemID);

            byte[] result = _httpClient.UploadData(_config.TestDataFetchUrl, body, 3);
            result = ChangeVersionFileName(result);

            return result;
        }

        /// <summary>
        /// 修改ZIP中的last_modified更名为version.txt
        /// </summary>
        private byte[] ChangeVersionFileName(byte[] data)
        {
            byte[] res;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data);
                ms.Position = 0;
                using (ZipArchive zipArchive = new ZipArchive(ms, ZipArchiveMode.Update))
                {
                    string version = "";
                    ZipArchiveEntry oldEntry = zipArchive.GetEntry("last_modified");
                    if (oldEntry != null)
                    {
                        using (StreamReader sr = new StreamReader(oldEntry.Open()))
                        {
                            version = sr.ReadToEnd();
                        }
                    }

                    ZipArchiveEntry newEntry = zipArchive.CreateEntry("version.txt");
                    using (StreamWriter writer = new StreamWriter(newEntry.Open())) 
                    {
                        writer.Write(version);
                    }

                    zipArchive.UpdateBaseStream();

                    int nowPos = (int)ms.Position;
                    res = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(res, 0, (int)ms.Length);
                    ms.Position = nowPos;
                }
            }

            return res;
        }

        //创建请求Body
        private string CreateRequestBody(string problemID)
        {
            return "pid=" + problemID;
        }

        public byte[] Fetch(int problemID)
        {
            return Fetch(problemID.ToString());
        }

        public byte[] Fetch(JudgeTask task)
        {
            return Fetch(task.ProblemID.ToString());
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
