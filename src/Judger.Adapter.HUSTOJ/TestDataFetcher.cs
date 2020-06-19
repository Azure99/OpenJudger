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
            InnerFile[] files = GetTestDataNameList(problemId)
                .Select(dataName => new InnerFile
                {
                    Name = dataName,
                    Data = GetTestDataFile(problemId, dataName)
                })
                .ToArray();

            return CreateZip(files, GetTestDataMd5(problemId));
        }

        private string[] GetTestDataNameList(string pid)
        {
            string requestBody = "gettestdatalist=1&pid=" + pid;
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            string[] dataNames = Regex.Split(response, "\r\n|\r|\n");

            return dataNames.Where(name => !string.IsNullOrEmpty(name)).ToArray();
        }

        private byte[] GetTestDataFile(string pid, string fileName)
        {
            string requestBody = "gettestdata=1&filename=" + pid + "/" + fileName;

            return HttpClient.UploadData(Config.TaskFetchUrl, requestBody, 3);
        }

        private byte[] CreateZip(InnerFile[] files, string dataVersion)
        {
            byte[] zipData;
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Create))
                {
                    foreach (InnerFile file in files)
                    {
                        ZipArchiveEntry entry = null;
                        if (file.Name.EndsWith(".in"))
                            entry = zip.CreateEntry("input/" + file.Name);
                        else if (file.Name.EndsWith(".out"))
                            entry = zip.CreateEntry("output/" + file.Name);
                        else if (CheckIsSpecialJudgeFile(file.Name))
                        {
                            entry = zip.CreateEntry("spj/" + SpjManager.SpjSourceFilename +
                                                    Path.GetExtension(file.Name));
                        }

                        if (entry == null)
                            continue;

                        using (Stream stream = entry.Open())
                        {
                            stream.Write(file.Data);
                        }
                    }

                    ZipArchiveEntry verEntry = zip.CreateEntry("version.txt");
                    using (StreamWriter writer = new StreamWriter(verEntry.Open()))
                    {
                        writer.Write(dataVersion);
                    }

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

        private string GetTestDataMd5(string pid)
        {
            string requestBody = $"gettestdatalist=1&pid={pid}&time=1";
            string response = HttpClient.UploadString(Config.TaskFetchUrl, requestBody, 3);

            return Md5Encrypt.EncryptToHexString(response);
        }

        private bool CheckIsSpecialJudgeFile(string fileName)
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

            return name == "spj" && extensionSet.Contains(extension);
        }

        private class InnerFile
        {
            public string Name { get; set; }
            public byte[] Data { get; set; }
        }
    }
}