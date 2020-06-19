using System;
using System.IO;
using Newtonsoft.Json;

namespace Judger.Models.Program
{
    /// <summary>
    /// 语言配置信息
    /// </summary>
    [Serializable]
    public class ProgramLangConfig : ILangConfig
    {
        /// <summary>
        /// 编程语言名称
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 判题时使用的临时目录
        /// </summary>
        public string JudgeDirectory { get; set; }
            = "JudgeTemp" + Path.DirectorySeparatorChar + "Judge";

        /// <summary>
        /// 是否需要编译(Python等脚本语言不需要)
        /// </summary>
        public bool NeedCompile { get; set; } = true;

        /// <summary>
        /// 写出的源文件名
        /// </summary>
        public string SourceCodeFileName { get; set; } = "";

        /// <summary>
        /// 源代码文件扩展名(使用|分隔), 用于Special Judge, 必须唯一
        /// </summary>
        public string SourceCodeFileExtension { get; set; } = "";

        /// <summary>
        /// 编译后可执行文件名
        /// </summary>
        public string ProgramFileName { get; set; } = "";

        /// <summary>
        /// 最大编译时间
        /// </summary>
        public int MaxCompileTime { get; set; } = 20000;

        /// <summary>
        /// 编译器路径
        /// </summary>
        public string CompilerPath { get; set; } = "";

        /// <summary>
        /// 编译器工作目录
        /// </summary>
        public string CompilerWorkDirectory { get; set; } = "";

        /// <summary>
        /// 编译器参数
        /// </summary>
        public string CompilerArgs { get; set; } = "";

        /// <summary>
        /// 运行器路径
        /// </summary>
        public string RunnerPath { get; set; } = "";

        /// <summary>
        /// 运行器工作目录
        /// </summary>
        public string RunnerWorkDirectory { get; set; } = "";

        /// <summary>
        /// 运行器参数
        /// </summary>
        public string RunnerArgs { get; set; } = "";

        /// <summary>
        /// I/O流是否使用UTF8编码
        /// </summary>
        public bool UseUtf8 { get; set; } = true;

        /// <summary>
        /// 是否运行在VM中
        /// </summary>
        /// Java等语言是
        public bool RunningInVm { get; set; }

        /// <summary>
        /// 被测试程序允许的最大输出内容长度
        /// </summary>
        /// 用于防止死循环输出
        public int OutputLimit { get; set; } = 67108864;

        /// <summary>
        /// 时间消耗
        /// </summary>
        /// 时间消耗=真实时间*补偿系数, 用于保证分布式判题机性能不同时减小误差
        public double TimeCompensation { get; set; } = 1.0;

        /// <summary>
        /// 是否为数据库配置
        /// </summary>
        [JsonIgnore]
        public bool IsDbConfig { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}