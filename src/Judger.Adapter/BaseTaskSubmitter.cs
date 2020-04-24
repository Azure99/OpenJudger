using Judger.Managers;
using Judger.Models;
using Judger.Utils;

namespace Judger.Adapter
{
    public abstract class BaseTaskSubmitter : ITaskSubmitter
    {
        /// <summary>
        /// 配置信息
        /// </summary>
        protected Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// Http客户端
        /// </summary>
        protected HttpWebClient HttpClient { get; } = ConfiguredClient.Create();

        /// <summary>
        /// 提交评测结果
        /// </summary>
        /// <param name="context">JudgeContext</param>
        /// <returns>是否成功</returns>
        public abstract bool Submit(JudgeContext context);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}