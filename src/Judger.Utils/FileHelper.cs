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
        /// 尝试读取文件的全部文本
        /// </summary>
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
        /// 尝试将文本写出到文件
        /// </summary>
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