namespace Judger.Adapter.Generic.Entity
{
    /// <summary>
    /// 响应码
    /// </summary>
    public enum ResponseCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 失败
        /// </summary>
        Fail = 1,

        /// <summary>
        /// Token错误
        /// </summary>
        WrongToken = 10,

        /// <summary>
        /// 当前没有任务
        /// </summary>
        NoTask = 11
    }
}