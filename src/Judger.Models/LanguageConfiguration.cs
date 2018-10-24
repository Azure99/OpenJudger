﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Judger.Models
{
    /// <summary>
    /// 语言配置信息
    /// </summary>
    [Serializable]
    public class LanguageConfiguration : ICloneable
    {
        /// <summary>
        /// 语言名称
        /// </summary>
        public string Language { get; set; } = "cpp";

        /// <summary>
        /// 是否需要编译(脚本语言不需要)
        /// </summary>
        public bool NeedCompile { get; set; } = true;

        /// <summary>
        /// 写出的源文件名
        /// </summary>
        public string SourceCodeFileName { get; set; } = "src.cpp";

        /// <summary>
        /// 最大编译时间
        /// </summary>
        public int MaxCompileTime { get; set; } = 20000;

        /// <summary>
        /// 判题时使用的目录
        /// </summary>
        public string JudgeDirectory { get; set; } 
            = "JudgeTemp" + System.IO.Path.DirectorySeparatorChar + "CppJudge";

        /// <summary>
        /// 编译器路径
        /// </summary>
        public string CompilerPath { get; set; } = @"Compilers\gcc\bin\g++.exe";

        /// <summary>
        /// 编译器工作目录
        /// </summary>
        public string CompilerWorkDirectory { get; set; } = @"Compilers\gcc\bin";

        /// <summary>
        /// 编译器参数
        /// </summary>
        public string CompilerArgs { get; set; } = "<tempdir>src.cpp -o2 <tempdir>program.exe";

        /// <summary>
        /// 运行器路径
        /// </summary>
        public string RunnerPath { get; set; } = "<tempdir>program.exe";

        /// <summary>
        /// 运行器工作目录
        /// </summary>
        public string RunnerWorkDirectory { get; set; } = @"Compilers\gcc\bin";

        /// <summary>
        /// 运行器参数
        /// </summary>
        public string RunnerArgs { get; set; } = "";

        /// <summary>
        /// 被测试程序允许的最大输出内容长度, 用于防止死循环输出
        /// </summary>
        public int OutputLimit { get; set; } = 67108864;

        /// <summary>
        /// 时间消耗=真实时间*补偿系数，用于保证分布式判题机性能不同时结果一致
        /// </summary>
        public double TimeCompensation { get; set; } = 1.0;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
