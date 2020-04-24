using Judger.Models.Exception;

namespace Judger.Core.Program.Internal.Entity
{
    /// <summary>
    /// 编译错误
    /// </summary>
    public class CompileException : JudgeException
    {
        public CompileException(string message) : base(message)
        { }
    }
}