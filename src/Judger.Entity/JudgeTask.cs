﻿using System;

namespace Judger.Entity
{
    /// <summary>
    /// 评测任务
    /// </summary>
    [Serializable]
    public class JudgeTask : ICloneable
    {
        /// <summary>
        /// 提交ID
        /// </summary>
        public int SubmitId { get; set; }

        /// <summary>
        /// 题目ID
        /// </summary>
        public int ProblemId { get; set; }

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
        /// 是否评测所有测试点(错误答案依旧评测)
        /// </summary>
        public bool JudgeAllCases { get; set; } = false;

        /// <summary>
        /// 评测类型
        /// </summary>
        public JudgeType JudgeType { get; set; } = JudgeType.ProgramJudge;

        /// <summary>
        /// 源代码
        /// </summary>
        public string SourceCode { get; set; } = "";

        /// <summary>
        /// 处理器亲和性(二进制表示)
        /// </summary>
        public IntPtr ProcessorAffinity { get; set; } = new IntPtr(1);

        /// <summary>
        /// 编程语言配置信息
        /// </summary>
        public ILangConfig LangConfig { get; set; }

        /// <summary>
        /// 判题所用临时目录
        /// </summary>                                                                                                                                                                                                                                                                                  '                                     
        public string TempJudgeDirectory { get; set; }

        public object Clone()
        {
            JudgeTask task = MemberwiseClone() as JudgeTask;
            task.LangConfig = LangConfig.Clone() as ILangConfig;
            return task;
        }
    }
}