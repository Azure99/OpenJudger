namespace Judger.Models.Exception
{
    /// <summary>
    /// 数据库驱动无效异常
    /// </summary>
    public class InvalidDbDriverException : BaseException
    {
        public InvalidDbDriverException(string message) : base(message)
        { }
    }
}