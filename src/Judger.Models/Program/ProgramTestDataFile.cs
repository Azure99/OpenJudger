namespace Judger.Models.Program
{
    /// <summary>
    /// 程序测试数据文件名
    /// </summary>
    /// 仅包含一组测试数据的输入文件、输出文件名
    public class ProgramTestDataFile
    {
        /// <summary>
        /// 测试数据名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标准输入文件名
        /// </summary>
        public string InputFile { get; set; }

        /// <summary>
        /// 标准输出文件名
        /// </summary>
        public string OutputFile { get; set; }
    }
}