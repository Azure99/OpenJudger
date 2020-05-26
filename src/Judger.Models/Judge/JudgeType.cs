namespace Judger.Models.Judge
{
    /// <summary>
    /// 评测类型
    /// </summary>
    public enum JudgeType
    {
        /// <summary>
        /// 传统程序评测
        /// </summary>
        ProgramJudge = 0,

        /// <summary>
        /// 特殊程序评测
        /// </summary>
        /// 输出交由SPJ程序评测
        SpecialJudge = 1,

        /// <summary>
        /// 数据库评测
        /// </summary>
        DbJudge = 2
    }
}