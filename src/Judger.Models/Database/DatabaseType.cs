namespace Judger.Models.Database
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// Mysql
        /// </summary>
        mysql = 1,

        /// <summary>
        /// Microsoft Sql Server
        /// </summary>
        /// 暂未适配
        mssql = 2,

        /// <summary>
        /// Oracle
        /// </summary>
        /// 暂未适配
        oracle = 3
    }
}