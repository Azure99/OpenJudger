using System;
using Judger.Models;

namespace Judger.Adapter
{
    /// <summary>
    /// TestDataFetcher接口
    /// </summary>
    /// 用于从后端拉取测试数据
    public interface ITestDataFetcher : IDisposable
    {
        /// <summary>
        /// 从后端拉取题目的测试数据
        /// </summary>
        /// <param name="context">评测上下文</param>
        /// <returns>byte[]形式的Zip文件</returns>
        byte[] Fetch(JudgeContext context);
    }
}