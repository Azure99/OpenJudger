using System;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace Judger.Core.Database.Internal
{
    /// <summary>
    /// 数据库驱动加载器
    /// </summary>
    public static class DbDriverLoader
    {
        /// <summary>
        /// 加载数据库驱动
        /// </summary>
        /// <param name="assemblyPath">数据库驱动程序集的路径</param>
        /// <returns>数据库驱动</returns>
        public static DbDriver Load(string assemblyPath)
        {
            assemblyPath = Path.GetFullPath(assemblyPath);

            Type connectionType = null;
            Type commandType = null;
            Assembly assembly = Assembly.LoadFile(assemblyPath);

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsClass)
                {
                    continue;
                }

                if (type.BaseType == typeof(DbConnection))
                {
                    connectionType = type;
                }
                else if (type.BaseType == typeof(DbCommand))
                {
                    commandType = type;
                }
            }

            if (connectionType == null || commandType == null)
            {
                throw new Exception("Driver is not available!");
            }

            return new DbDriver(connectionType, commandType);
        }
    }
}
