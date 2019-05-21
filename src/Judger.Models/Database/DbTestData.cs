namespace Judger.Models.Database
{
    /// <summary>
    /// 数据库测试数据
    /// </summary>
    public class DbTestData
    {
        /// <summary>
        /// 测试数据名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据库初始化命令
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// 数据库操作标程
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// 数据库查询标程
        /// </summary>
        public string Query { get; set; }
    }
}