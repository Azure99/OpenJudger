namespace Judger.Entity
{
    /// <summary>
    /// 评测类型
    /// </summary>
    public enum JudgeType
    {
        /// <summary>
        /// 传统程序评测
        /// </summary>
        ProgramJudge,
        
        /// <summary>
        /// 特殊程序评测(交由SPJ程序评测)
        /// </summary>
        SpecialJudge,
        
        /// <summary>
        /// 数据库评测
        /// </summary>
        DbJudge
    }
}