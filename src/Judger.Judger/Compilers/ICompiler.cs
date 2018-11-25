using System;
using System.Collections.Generic;
using System.Text;
using Judger.Models;

namespace Judger.Judger.Compilers
{
    /// <summary>
    /// 编译器接口
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// 评测任务
        /// </summary>
        JudgeTask JudgeTask { get; set; }

        /// <summary>
        /// 编译任务代码
        /// </summary>
        string Compile();
    }
}
