using System;
using System.Collections.Generic;
using System.Text;
using Judger.Judger.Models;

namespace Judger.Judger
{
    /// <summary>
    /// 单例Judger接口
    /// </summary>
    public interface ISingleJudger
    {
        /// <summary>
        /// 运行器路径
        /// </summary>
        string RunnerPath { get; set; }

        /// <summary>
        /// 运行器工作目录
        /// </summary>
        string RunnerWorkDirectory { get; set; }

        /// <summary>
        /// 运行器运行参数
        /// </summary>
        string RunnerArgs { get; set; }

        /// <summary>
        /// 时间限制
        /// </summary>
        int TimeLimit { get; set; }

        /// <summary>
        /// 内存限制
        /// </summary>
        int MemoryLimit { get; set; }
        
        /// <summary>
        /// 输出限制
        /// </summary>
        int OutputLimit { get; set; }

        /// <summary>
        /// 处理器亲和性
        /// </summary>
        IntPtr ProcessorAffinity { get; set; }

        /// <summary>
        /// 评测
        /// </summary>
        /// <param name="input">答案输入</param>
        /// <param name="output">答案输出</param>
        /// <returns>单组评测结果</returns>
        SingleJudgeResult Judge(string input, string output);
    }
}
