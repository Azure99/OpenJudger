using System;
using Judger.Models;
using Judger.Models.Judge;

namespace Judger.Adapter.SDNUOJ
{
    /// <summary>
    /// JudgeResult提交器
    /// </summary>
    public class TaskSubmitter : BaseTaskSubmitter
    {
        public override bool Submit(JudgeContext context)
        {
            HttpClient.CookieContainer = Authenticator.Instance.CookieContainer;
            HttpClient.UploadString(Config.ResultSubmitUrl, CreateResultBody(context.Result), 3);
            return true;
        }

        /// <summary>
        /// 根据JudgeResult生成用于提交的数据
        /// </summary>
        private string CreateResultBody(JudgeResult result)
        {
            int resultCode;
            switch (result.ResultCode)
            {
                case JudgeResultCode.Accepted:
                    resultCode = 10;
                    break;
                case JudgeResultCode.CompileError:
                    resultCode = 3;
                    break;
                case JudgeResultCode.JudgeFailed:
                    resultCode = 255;
                    break;
                case JudgeResultCode.MemoryLimitExceed:
                    resultCode = 6;
                    break;
                case JudgeResultCode.OutputLimitExceed:
                    resultCode = 7;
                    break;
                case JudgeResultCode.PresentationError:
                    resultCode = 9;
                    break;
                case JudgeResultCode.RuntimeError:
                    resultCode = 4;
                    break;
                case JudgeResultCode.TimeLimitExceed:
                    resultCode = 5;
                    break;
                case JudgeResultCode.WrongAnswer:
                    resultCode = 8;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            string sid = result.SubmitId;
            string detail = result.JudgeDetail;
            int timeCost = result.TimeCost;
            int memCost = result.MemoryCost;
            string pid = result.ProblemId;
            string username = result.Author;
            string body =
                $"sid={sid}&resultcode={resultCode}&detail={detail}" +
                $"&timecost={timeCost}&memorycost={memCost}&pid={pid}&username={username}";

            return body;
        }
    }
}