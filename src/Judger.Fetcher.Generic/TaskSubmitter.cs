using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Judger.Models;

namespace Judger.Fetcher.Generic
{
    /// <summary>
    /// JudgeResult提交器
    /// </summary>
    public class TaskSubmitter : BaseTaskSubmitter
    {
        public TaskSubmitter()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        public override bool Submit(JudgeResult result)
        {
            HttpClient.UploadString(Config.ResultSubmitUrl, CreateResultBody(result), 3);
            return true;
        }

        /// <summary>
        /// 根据JudgeResult生成用于提交的Body
        /// </summary>
        private string CreateResultBody(JudgeResult result)
        {
            JObject body = JObject.FromObject(result);
            body.Add("JudgerName", Config.JudgerName);
            body.Add("Token", Token.Create());

            return body.ToString();
        }
    }
}
