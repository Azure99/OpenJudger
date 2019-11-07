using System.IO;
using System.IO.Compression;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// TestData获取器
    /// </summary>
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
            string body = CreateRequestBody(problemId);

            byte[] result = HttpClient.UploadData(Config.TestDataFetchUrl, body, 3);
            result = ChangeVersionFileName(result);

            return result;
        }

        //创建请求Body
        private string CreateRequestBody(string problemId)
        {
            return "pid=" + problemId;
        }

        /// <summary>
        /// 修改ZIP中的last_modified更名为version.txt
        /// </summary>
        private byte[] ChangeVersionFileName(byte[] data)
        {
            byte[] res;
            using (var ms = new MemoryStream())
            {
                ms.Write(data);
                ms.Position = 0;
                using (var zipArchive = new ZipArchive(ms, ZipArchiveMode.Update))
                {
                    var version = "";
                    ZipArchiveEntry oldEntry = zipArchive.GetEntry("last_modified");
                    if (oldEntry != null)
                        using (var sr = new StreamReader(oldEntry.Open()))
                            version = sr.ReadToEnd();

                    ZipArchiveEntry newEntry = zipArchive.CreateEntry("version.txt");
                    using (var writer = new StreamWriter(newEntry.Open())) writer.Write(version);

                    zipArchive.UpdateBaseStream();

                    var nowPos = (int) ms.Position;
                    res = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(res, 0, (int) ms.Length);
                    ms.Position = nowPos;
                }
            }

            return res;
        }
    }
}