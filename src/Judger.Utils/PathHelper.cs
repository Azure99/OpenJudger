using System;
using System.IO;
using System.Linq;

namespace Judger.Utils
{
    /// <summary>
    /// 路径辅助类
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// 获取以程序所在目录(而非工作目录)为准的绝对路径
        /// </summary>
        /// <param name="path">相对路径(绝对路径不处理)</param>
        /// <returns>绝对路径</returns>
        public static string GetBaseAbsolutePath(string path)
        {
            // 绝对路径不处理
            if (path.IndexOf(':') != -1 || path.StartsWith('/') || path.StartsWith('\\'))
                return path;

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, path);
        }

        /// <summary>
        /// 在指定目录下忽略大小写地获取文件路径
        /// </summary>
        /// <param name="directory">目录</param>
        /// <param name="filename">文件名</param>
        /// <returns>文件路径</returns>
        public static string FindFileIgnoreCase(string directory, string filename)
        {
            return Directory.GetFiles(directory).FirstOrDefault(
                file => Path.GetFileName(file).EqualsIgnoreCase(filename));
        }
    }
}