using System;
using Judger.Models;

namespace Judger.Adapter
{
    public interface ITestDataFetcher : IDisposable
    {
        /// <summary>
        /// 取回数据
        /// </summary>
        /// <param name="context">JudgeContext</param>
        /// <returns>byte[]形式的ZIP文件</returns>
        byte[] Fetch(JudgeContext context);
    }
}