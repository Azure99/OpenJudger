namespace Judger.Models.Exception
{
    /// <summary>
    /// 语言未找到
    /// </summary>
    public class LanguageNotFoundException : JudgeException
    {
        /// <summary>
        /// 编程语言未找到
        /// </summary>
        /// <param name="message">信息</param>
        public LanguageNotFoundException(string message) : base(message)
        { }
    }
}