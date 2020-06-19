namespace Judger.Models.Exception
{
    /// <summary>
    /// 预料中的评测终止
    /// </summary>
    /// 如编译失败, 时间超限等
    public class ExpectedJudgeStopException : BaseException
    {
        public ExpectedJudgeStopException(string message) : base(message)
        { }
    }
}