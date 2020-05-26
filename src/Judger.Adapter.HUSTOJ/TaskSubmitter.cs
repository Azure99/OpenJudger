using Judger.Models;
using Judger.Models.Judge;

namespace Judger.Adapter.HUSTOJ
{
    public class TaskSubmitter : BaseTaskSubmitter
    {
        public override bool Submit(JudgeContext context)
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
            HttpClient.UploadString(Config.TaskFetchUrl, CreateResultBody(context.Result), 3);
            return true;
        }

        private string CreateResultBody(JudgeResult result)
        {
            int resultCode;
            switch (result.ResultCode)
            {
                case JudgeResultCode.Accepted:
                    resultCode = 4;
                    break;
                case JudgeResultCode.CompileError:
                    resultCode = 11;
                    break;
                case JudgeResultCode.JudgeFailed:
                    resultCode = 11;
                    break;
                case JudgeResultCode.MemoryLimitExceed:
                    resultCode = 8;
                    break;
                case JudgeResultCode.OutputLimitExceed:
                    resultCode = 9;
                    break;
                case JudgeResultCode.PresentationError:
                    resultCode = 5;
                    break;
                case JudgeResultCode.RuntimeError:
                    resultCode = 10;
                    break;
                case JudgeResultCode.TimeLimitExceed:
                    resultCode = 7;
                    break;
                case JudgeResultCode.WrongAnswer:
                    resultCode = 6;
                    break;
                default:
                    resultCode = 0;
                    break;
            }

            return $"update_solution=1&sid={result.SubmitId}&result={resultCode}&time={result.TimeCost}" +
                   $"&memory={result.MemoryCost}&sim=0&simid=0&pass_rate={result.PassRate}";
        }
    }
}