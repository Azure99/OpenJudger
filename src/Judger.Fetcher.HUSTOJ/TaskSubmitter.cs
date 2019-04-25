using Judger.Entity;

namespace Judger.Fetcher.HUSTOJ
{
    public class TaskSubmitter : BaseTaskSubmitter
    {
        public override bool Submit(JudgeResult result)
        {
            HttpClient.CookieContainer = Authenticator.Singleton.CookieContainer;
            HttpClient.UploadString(Config.TaskFetchUrl, CreateResultBody(result), 3);
            return true;
        }

        /// <summary>
        /// 根据JudgeResult生成用于提交的Body
        /// </summary>
        private string CreateResultBody(JudgeResult result)
        {
            int resultCode = 0;
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
            }

            string body = string.Format(
                "update_solution=1&sid={0}&result={1}&time={2}&memory={3}&sim=0&simid=0&pass_rate={4}",
                result.SubmitId, resultCode, result.TimeCost, result.MemoryCost, result.PassRate);

            return body;
        }
    }
}