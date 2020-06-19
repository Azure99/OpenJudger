using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Judger.Models.Exception;

namespace Judger.Core.Database.Internal
{
    /// <summary>
    /// 数据库驱动加载器
    /// </summary>
    public static class DbDriverLoader
    {
        public static DbDriver Load(string assemblyPath)
        {
            assemblyPath = Path.GetFullPath(assemblyPath);

            Type connectionType = null;
            Type commandType = null;
            Assembly assembly = Assembly.LoadFile(assemblyPath);

            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!type.IsClass)
                    continue;

                if (type.BaseType == typeof(DbConnection))
                    connectionType = type;
                else if (type.BaseType == typeof(DbCommand))
                    commandType = type;
            }

            if (connectionType == null || commandType == null)
                throw new InvalidDbDriverException($"Driver: {assemblyPath} is not available!");

            return new DbDriver(connectionType, commandType);
        }
    }
}