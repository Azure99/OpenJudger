﻿using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;

namespace Judger.Fetcher.SDNUOJ
{
    /// <summary>
    /// JudgeResult提交器
    /// </summary>
    public class TaskSubmitter : BaseTaskSubmitter
    {
        public override bool Submit(JudgeResult result)
        {
            Client.CookieContainer = Authenticator.Singleton.CookieContainer;
            Client.UploadString(Config.ResultSubmitUrl, GetDataForSubmit(result), 3);
            return true;
        }

        /// <summary>
        /// 根据JudgeResult生成用于提交的数据
        /// </summary>
        /// <param name="result">JudgeResult</param>
        private string GetDataForSubmit(JudgeResult result)
        {
            int resultCode = 0;
            switch (result.ResultCode)
            {
                case JudgeResultCode.Accepted: resultCode = 10; break;
                case JudgeResultCode.CompileError: resultCode = 3; break;
                case JudgeResultCode.JudgeFailed: resultCode = 255; break;
                case JudgeResultCode.MemoryLimitExceed: resultCode = 6; break;
                case JudgeResultCode.OutputLimitExceed: resultCode = 7; break;
                case JudgeResultCode.PresentationError: resultCode = 9; break;
                case JudgeResultCode.RuntimeError: resultCode = 4; break;
                case JudgeResultCode.TimeLimitExceed: resultCode = 5; break;
                case JudgeResultCode.WrongAnswer: resultCode = 8; break;
            }

            int sid = result.SubmitID;
            string detail = result.JudgeDetail;
            int timeCost = result.TimeCost;
            int memCost = result.MemoryCost;
            int pid = result.ProblemID;
            string username = result.Author;
            string data = string.Format("sid={0}&resultcode={1}&detail={2}&timecost={3}&memorycost={4}&pid={5}&username={6}",
                                          sid, resultCode, detail, timeCost, memCost, pid, username);

            return data.ToString();
        }
    }
}
