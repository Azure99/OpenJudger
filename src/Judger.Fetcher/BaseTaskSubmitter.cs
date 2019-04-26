using Judger.Entity;
using Judger.Managers;
using Judger.Utils;

namespace Judger.Fetcher
{
    /// <summary>
    /// TaskSubmitter基类
    /// </summary>
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
        /// <param name="result">JudgeResult</param>
        /// <returns>是否成功</returns>
        public abstract bool Submit(JudgeResult result);

        public virtual void Dispose()
        {
            HttpClient.Dispose();
        }
    }
}