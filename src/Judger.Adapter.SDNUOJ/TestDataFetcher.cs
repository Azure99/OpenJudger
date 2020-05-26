using System.IO;
using System.IO.Compression;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter.SDNUOJ
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
            string body = CreateRequestBody(problemId);

            byte[] result = HttpClient.UploadData(Config.TestDataFetchUrl, body, 3);
            result = ChangeVersionFileName(result);

            return result;
        }

        private string CreateRequestBody(string problemId)
        {
            return "pid=" + problemId;
        }

        /// <summary>
        /// 将Zip中的last_modified文件更名为version.txt
        /// </summary>
        private byte[] ChangeVersionFileName(byte[] data)
        {
            byte[] res;
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(data);
                stream.Position = 0;
                using (ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Update))
                {
                    string version = "";
                    ZipArchiveEntry oldEntry = zipArchive.GetEntry("last_modified");
                    if (oldEntry != null)
                    {
                        using (StreamReader reader = new StreamReader(oldEntry.Open()))
                            version = reader.ReadToEnd();
                    }

                    ZipArchiveEntry newEntry = zipArchive.CreateEntry("version.txt");
                    using (StreamWriter writer = new StreamWriter(newEntry.Open()))
                        writer.Write(version);

                    zipArchive.UpdateBaseStream();

                    int nowPos = (int) stream.Position;
                    res = new byte[stream.Length];
                    stream.Position = 0;
                    stream.Read(res, 0, (int) stream.Length);
                    stream.Position = nowPos;
                }
            }

            return res;
        }
    }
}