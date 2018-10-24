using System;
using System.Collections.Generic;
using System.Text;

namespace Judger.Judger.Compilers
{
    /// <summary>
    /// 编译器接口
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// 编译器路径
        /// </summary>
        string CompilerPath { get; set; }

        /// <summary>
        /// 编译器工作目录
        /// </summary>
        string CompilerWorkDirectory { get; set; }

        /// <summary>
        /// 编译超时时间
        /// </summary>
        int TimeLimit { get; set; }

        /// <summary>
        /// 处理器亲和性
        /// </summary>
        IntPtr ProcessorAffinity { get; set; }

        /// <summary>
        /// 编译指定代码
        /// </summary>
        /// <param name="args">调用编译器时传递的参数</param>
        /// <returns></returns>
        string Compile(string args);
    }
}
