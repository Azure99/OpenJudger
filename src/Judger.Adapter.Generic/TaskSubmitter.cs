using Judger.Adapter.Generic.Entity;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;
using Judger.Utils;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic
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

        /// <summary>
        /// 提交评测任务
        /// 此方法在评测完成后回调，以向Web后端提交结果
        /// 评测结果可从JudgeContext.Result中获取
        /// </summary>
        /// <param name="context">评测任务</param>
        /// <returns>提交是否成功</returns>
        public override bool Submit(JudgeContext context)
        {
            string url = Config.ResultSubmitUrl;
            string requestBody = CreateRequestBody(context.Result);

            string responseData = HttpClient.UploadString(url, requestBody, 3);
            var response = Json.DeSerialize<ServerResponse>(responseData);

            if (response.Code == ResponseCode.Fail || response.Code == ResponseCode.WrongToken)
                throw new AdapterException(context.Result.SubmitId + " submit failed: " + response.Message);

            return true;
        }

        /// <summary>
        /// 创建提交测试结果的请求
        /// </summary>
        /// <returns>提交测试结果的请求</returns>
        private string CreateRequestBody(JudgeResult result)
        {
            var judgeResult = new InnerJudgeResult
            {
                SubmitId = result.SubmitId,
                JudgeDetail = result.JudgeDetail,
                MemoryCost = result.MemoryCost,
                PassRate = result.PassRate,
                ResultCode = result.ResultCode,
                TimeCost = result.TimeCost
            };

            JObject requestBody = JObject.FromObject(new
            {
                result = judgeResult
            });

            Token.AddTokenToJObject(requestBody);

            return requestBody.ToString();
        }
    }
}