using System;
using System.IO;

namespace Judger.Utils
{
    /// <summary>
    /// 路径辅助类
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// 获取以程序所在目录(非工作目录)为准的绝对路径
        /// </summary>
        /// <param name="path">相对路径(绝对路径不处理)</param>
        /// <returns>绝对路径</returns>
        public static string GetBaseAbsolutePath(string path)
        {
            // 绝对路径不处理
            if (path.IndexOf(':') != -1 || path.StartsWith('/') || path.StartsWith('\\')) 
            {
                return path;
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }
}
