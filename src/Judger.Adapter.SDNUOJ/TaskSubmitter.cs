using System.Collections.Generic;
using Judger.Models;
using Judger.Models.Judge;

namespace Judger.Adapter.SDNUOJ
{
    public class TaskSubmitter : BaseTaskSubmitter
    {
        private readonly Dictionary<JudgeResultCode, int> _resultCodeDic = new Dictionary<JudgeResultCode, int>
        {
            {JudgeResultCode.Accepted, 10},
            {JudgeResultCode.CompileError, 3},
            {JudgeResultCode.JudgeFailed, 255},
            {JudgeResultCode.MemoryLimitExceed, 6},
            {JudgeResultCode.OutputLimitExceed, 7},
            {JudgeResultCode.PresentationError, 9},
            {JudgeResultCode.RuntimeError, 4},
            {JudgeResultCode.TimeLimitExceed, 5},
            {JudgeResultCode.WrongAnswer, 8}
        };

        public override void Submit(JudgeContext context)
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
            HttpClient.UploadString(Config.ResultSubmitUrl, CreateResultBody(context.Result), 3);
        }

        private string CreateResultBody(JudgeResult result)
        {
            int resultCode = _resultCodeDic[result.ResultCode];

            return $"sid={result.SubmitId}&resultcode={resultCode}&detail={result.JudgeDetail}" +
                   $"&timecost={result.TimeCost}&memorycost={result.MemoryCost}" +
                   $"&pid={result.ProblemId}&username={result.Author}";
        }
    }
}