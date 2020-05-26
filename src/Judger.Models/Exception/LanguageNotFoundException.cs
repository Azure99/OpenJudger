namespace Judger.Models.Exception
{
    /// <summary>
    /// 编程语言未找到异常
    /// </summary>
    public class LanguageNotFoundException : JudgeException
    {
        public LanguageNotFoundException(string message) : base(message)
        { }
    }
}