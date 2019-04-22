namespace Judger.Entity.Database
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
        /// 测试输入(操作前的数据库)
        /// </summary>
        public string Input { get; set; }

        /// <summary>
        /// 测试输出(操作后的数据库)
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// 测试查询
        /// </summary>
        public string Query { get; set; }
    }
}
