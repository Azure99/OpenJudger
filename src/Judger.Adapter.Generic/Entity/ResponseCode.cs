namespace Judger.Adapter.Generic.Entity
{
    /// <summary>
    /// 响应码
    /// </summary>
    public enum ResponseCode
    {
        // 成功
        Success = 0,
        // 失败
        Fail = 1,
        // Token错误
        WrongToken = 10,
        // 当前没有任务
        NoTask = 11
    }
}