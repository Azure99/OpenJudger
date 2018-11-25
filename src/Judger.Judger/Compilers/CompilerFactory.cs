using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;

namespace Judger.Judger.Compilers
{
    public static class CompilerFactory
    {
        /// <summary>
        /// 创建Compiler
        /// </summary>
        public static ICompiler Create(JudgeTask task)
        {
            ICompiler compiler = new Compiler(task);

            return compiler;
        }
    }
}
