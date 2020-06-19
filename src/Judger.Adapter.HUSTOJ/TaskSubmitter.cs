using System.Collections.Generic;
using Judger.Models;
using Judger.Models.Judge;

namespace Judger.Adapter.HUSTOJ
{
    public class TaskSubmitter : BaseTaskSubmitter
    {
        private readonly Dictionary<JudgeResultCode, int> _resultCodeDic = new Dictionary<JudgeResultCode, int>
        {
            {JudgeResultCode.Accepted, 4},
            {JudgeResultCode.CompileError, 11},
            {JudgeResultCode.JudgeFailed, 11},
            {JudgeResultCode.MemoryLimitExceed, 8},
            {JudgeResultCode.OutputLimitExceed, 9},
            {JudgeResultCode.PresentationError, 5},
            {JudgeResultCode.RuntimeError, 10},
            {JudgeResultCode.TimeLimitExceed, 7},
            {JudgeResultCode.WrongAnswer, 6}
        };

        public override void Submit(JudgeContext context)
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
            HttpClient.UploadString(Config.TaskFetchUrl, CreateResultBody(context.Result), 3);
        }

        private string CreateResultBody(JudgeResult result)
        {
            int resultCode = _resultCodeDic[result.ResultCode];

            return $"update_solution=1&sid={result.SubmitId}&result={resultCode}&time={result.TimeCost}" +
                   $"&memory={result.MemoryCost}&sim=0&simid=0&pass_rate={result.PassRate}";
        }
    }
}