using System;
using System.Collections.Generic;
using System.Text;

namespace Judger.Entity
{
    /// <summary>
    /// SpecialJudge程序文件
    /// </summary>
    public class SpecialJudgeProgram
    {
        /// <summary>
        /// SPJ程序的语言信息
        /// </summary>
        public ProgramLangConfig LangConfiguration { get; set; }

        /// <summary>
        /// SPJ程序
        /// </summary>
        public byte[] Program { get; set; }
    }
}
