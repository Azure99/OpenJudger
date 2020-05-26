namespace Judger.Models.Program
{
    /// <summary>
    /// 程序测试数据
    /// </summary>
    /// 包含一组测试数据的完整输入输出
    public class ProgramTestData
    {
        /// <summary>
        /// 测试数据名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标准输入
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// 标准输出
        /// </summary>
        public string Output { get; set; }
    }
}