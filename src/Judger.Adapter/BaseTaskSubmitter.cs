using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    /// <summary>
    /// TaskSubmitter基类
    /// </summary>
    /// 用于向后端提交评测结果
    public abstract class BaseTaskSubmitter : ITaskSubmitter
    {
        protected Configuration Config { get; } = ConfigManager.Config;

        protected HttpWebClient HttpClient { get; } = WebClientFactory.Create();

        /// <summary>
        /// 向后端提交评测结果
        /// </summary>
        /// <param name="context">评测上下文</param>
        /// <returns>是否成功</returns>
        public abstract bool Submit(JudgeContext context);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}