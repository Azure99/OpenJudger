using System;
using Judger.Models;

namespace Judger.Adapter
{
    /// <summary>
    /// TestDataFetcher接口
    /// </summary>
    /// 用于从服务端拉取测试数据
    public interface ITestDataFetcher : IDisposable
    {
        /// <summary>
        /// 从服务端获取题目的测试数据
        /// </summary>
        /// <param name="context">评测上下文</param>
        /// <returns>二进制的zip文件</returns>
        /// 当Judger获得新任务, 且本地不存在测试数据或数据过期时, 会自动调用此方法拉取测试数据
        /// 测试数据应当打包在zip文件中并以二进制保存
        byte[] Fetch(JudgeContext context);
    }
}