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
            Client.DefaultContentType = "application/json";
        }

        public override bool Submit(JudgeResult result)
        {
            Client.UploadString(Config.ResultSubmitUrl, GetDataForSubmit(result), 3);
            return true;
        }

        /// <summary>
        /// 根据JudgeResult生成用于提交的数据
        /// </summary>
        /// <param name="result">JudgeResult</param>
        private string GetDataForSubmit(JudgeResult result)
        {
            JObject obj = JObject.FromObject(result);
            obj.Add("JudgerName", Config.JudgerName);
            obj.Add("Token", Token.Create());

            return obj.ToString();
        }
    }
}
