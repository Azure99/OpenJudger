namespace Judger.Models.Program
{
    /// <summary>
    /// Special Judge源文件
    /// </summary>
    public class SpecialJudgeSourceFile
    {
        /// <summary>
        /// 编程语言配置
        /// </summary>
        public ProgramLangConfig LangConfig { get; set; }

        /// <summary>
        /// 源代码
        /// </summary>
        public string SourceCode { get; set; }
    }
}