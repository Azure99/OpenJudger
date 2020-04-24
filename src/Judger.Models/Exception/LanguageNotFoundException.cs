namespace Judger.Models.Exception
{
    /// <summary>
    /// 编程语言未找到
    /// </summary>
    public class LanguageNotFoundException : JudgeException
    {
        public LanguageNotFoundException(string message) : base(message)
        { }
    }
}