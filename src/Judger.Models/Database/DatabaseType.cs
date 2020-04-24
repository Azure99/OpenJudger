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
        /// Microsoft Sql Server (暂未适配)
        /// </summary>
        mssql = 2,

        /// <summary>
        /// Oracle (暂未适配)
        /// </summary>
        //暂未适配
        oracle = 3
    }
}