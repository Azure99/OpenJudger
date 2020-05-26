using System.IO.Compression;
using System.Reflection;

namespace Judger.Utils
{
    /// <summary>
    /// ZipArchive扩展
    /// </summary>
    public static class ZipArchiveExtends
    {
        private const string METHOD_WRITE_FILE = "WriteFile";

        /// <summary>
        /// 通过反射强制更新基础流
        /// </summary>
        public static void UpdateBaseStream(this ZipArchive zipArchive)
        {
            foreach (MethodInfo method in zipArchive.GetType().GetRuntimeMethods())
            {
                if (method.Name == METHOD_WRITE_FILE)
                {
                    method.Invoke(zipArchive, new object[0]);
                    break;
                }
            }
        }
    }
}