using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// JudgeResult提交器
    /// </summary>
    public class TaskSubmitter : ITaskSubmitter
    {
        private readonly Configuration _config = ConfigManager.Config;

        public bool Submit(JudgeResult result)
        {
            using (HttpWebClient client = ConfiguredClient.Create())
            {
                client.DefaultContentType = "application/json";

                client.UploadString(_config.ResultSubmitUrl, GetDataForSubmit(result), 3);
            }
            return true;
        }

        /// <summary>
        /// 根据JudgeResult生成用于提交的数据
        /// </summary>
        /// <param name="result">JudgeResult</param>
        private string GetDataForSubmit(JudgeResult result)
        {
            JObject obj = JObject.FromObject(result);
            obj.Add("JudgerName", ConfigManager.Config.JudgerName);
            obj.Add("Token", Token.Create());

            return obj.ToString();
        }

        public void Dispose()
        {

        }
    }
}
