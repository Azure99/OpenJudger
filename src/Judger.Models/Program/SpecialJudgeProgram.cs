namespace Judger.Models.Program
{
    /// <summary>
    /// Special Judge程序文件
    /// </summary>
    public class SpecialJudgeProgram
    {
        /// <summary>
        /// SPJ程序的语言信息
        /// </summary>
        public ProgramLangConfig LangConfig { get; set; }

        /// <summary>
        /// 二进制SPJ程序
        /// </summary>
        public byte[] Program { get; set; }
    }
}