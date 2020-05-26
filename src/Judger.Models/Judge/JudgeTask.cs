using System;

namespace Judger.Models.Judge
{
    /// <summary>
    /// 评测任务
    /// </summary>
    [Serializable]
    public class JudgeTask : ICloneable
    {
        /// <summary>
        /// 提交Id
        /// </summary>
        public string SubmitId { get; set; }

        /// <summary>
        /// 题目Id
        /// </summary>
        public string ProblemId { get; set; }

        /// <summary>
        /// 测试数据版本号
        /// </summary>
        public string DataVersion { get; set; }

        /// <summary>
        /// 提交者
        /// </summary>
        public string Author { get; set; } = "";

        /// <summary>
        /// 提交语言
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 时间限制
        /// </summary>
        public int TimeLimit { get; set; } = 1000;

        /// <summary>
        /// 内存限制
        /// </summary>
        public int MemoryLimit { get; set; } = 262144;

        /// <summary>
        /// 是否评测所有测试用例
        /// </summary>
        /// 出现错误的用例时继续评测
        /// OI赛制及编程考试需要
        public bool JudgeAllCases { get; set; }

        /// <summary>
        /// 评测类型
        /// </summary>
        public JudgeType JudgeType { get; set; } = JudgeType.ProgramJudge;

        /// <summary>
        /// 源代码
        /// </summary>
        public string SourceCode { get; set; } = "";

        /// <summary>
        /// 处理器亲和性
        /// </summary>
        /// 使用二进制表示
        /// 低位到高位, 每一位标志一个核心是否可用
        public IntPtr ProcessorAffinity { get; set; } = new IntPtr(1);

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}