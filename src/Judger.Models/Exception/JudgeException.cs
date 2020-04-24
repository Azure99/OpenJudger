namespace Judger.Models.Exception
{
    /// <summary>
    /// 判题异常
    /// </summary>
    public class JudgeException : BaseException
    {
        public JudgeException(string message) : base(message)
        { }
    }
}