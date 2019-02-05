using System;
using System.IO;
using System.Text;

namespace Judger.Utils
{
    /// <summary>
    /// 文件辅助类
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// 尝试读取文件文本
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="text">文本内容</param>
        /// <returns>是否成功</returns>
        public static bool TryReadAllText(string path, out string contents)
        {
            try
            {
                contents = File.ReadAllText(path, Encoding.UTF8);
                return true;
            }
            catch
            {
                contents = "";
                return false;
            }
        }

        /// <summary>
        /// 尝试写出文本文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="text">欲写入的文本</param>
        /// <returns>是否成功</returns>
        public static bool TryWriteAllText(string path, string contents)
        {
            try
            {
                File.WriteAllText(path, contents, Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
