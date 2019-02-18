using System;
using Judger.Entity;

namespace Judger.Core.Program.Entity
{
    public class CompileException : JudgeException
    {
        public CompileException(string message) : base(message)
        { }
    }
}
