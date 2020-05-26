namespace Judger.Models.Judge
{
    /// <summary>
    /// 判题结果码
    /// </summary>
    public enum JudgeResultCode
    {
        /// <summary>
        /// 判题失败
        /// </summary>
        JudgeFailed = -1,

        /// <summary>
        /// 正确
        /// </summary>
        Accepted = 0,

        /// <summary>
        /// 答案错误
        /// </summary>
        WrongAnswer = 1,

        /// <summary>
        /// 编译错误
        /// </summary>
        CompileError = 2,

        /// <summary>
        /// 运行时错误
        /// </summary>
        RuntimeError = 3,

        /// <summary>
        /// 时间超限
        /// </summary>
        TimeLimitExceed = 4,

        /// <summary>
        /// 内存超限
        /// </summary>
        MemoryLimitExceed = 5,

        /// <summary>
        /// 输出超限
        /// </summary>
        /// 输出了过多的内容
        OutputLimitExceed = 6,

        /// <summary>
        /// 格式错误
        /// </summary>
        /// 输出了多余的空格或换行符
        PresentationError = 7
    }
}