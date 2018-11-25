using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;

namespace Judger.Judger
{
    /// <summary>
    /// 单例Judger工厂
    /// </summary>
    public static class SingleJudgerFactory
    {
        /// <summary>
        /// 创建单例Judger
        /// </summary>
        public static ISingleJudger Create(JudgeTask task)
        {
            ISingleJudger judger = new SingleJudger(task);

            return judger;
        }
    }
}
