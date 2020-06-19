using Judger.Adapter.Generic.Entity;
using Judger.Models;
using Judger.Models.Exception;
using Judger.Models.Judge;
using Judger.Utils;
using Newtonsoft.Json.Linq;

namespace Judger.Adapter.Generic
{
    public class TaskSubmitter : BaseTaskSubmitter
    {
        public TaskSubmitter()
        {
            HttpClient.DefaultContentType = "application/json";
        }

        public override void Submit(JudgeContext context)
        {
            string requestBody = CreateRequestBody(context.Result);
            string jsonResp = HttpClient.UploadString(Config.ResultSubmitUrl, requestBody, 3);

            ServerResponse response = Json.DeSerialize<ServerResponse>(jsonResp);
            if (response.Code == ResponseCode.Fail || response.Code == ResponseCode.WrongToken)
                throw new AdapterException(context.Result.SubmitId + " submit failed: " + response.Message);
        }

        private string CreateRequestBody(JudgeResult result)
        {
            InnerJudgeResult innerResult = new InnerJudgeResult
            {
                SubmitId = result.SubmitId,
                JudgeDetail = result.JudgeDetail,
                MemoryCost = result.MemoryCost,
                PassRate = result.PassRate,
                ResultCode = result.ResultCode,
                TimeCost = result.TimeCost
            };

            JObject requestJObject = TokenUtil.CreateJObject();
            requestJObject.Add("result", Json.Serialize(innerResult));

            return requestJObject.ToString();
        }
    }
}