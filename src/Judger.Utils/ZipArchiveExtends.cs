using System.IO.Compression;
using System.Reflection;

namespace Judger.Utils
{
    /// <summary>
    /// ZipArchive扩展类
    /// </summary>
    public static class ZipArchiveExtends
    {
        /// <summary>
        /// 强制更新基础流
        /// </summary>
        /// <param name="zipArchive">ZipArchive实例</param>
        public static void UpdateBaseStream(this ZipArchive zipArchive)
        {
            foreach (MethodInfo method in zipArchive.GetType().GetRuntimeMethods())
            {
                if (method.Name == "WriteFile")
                {
                    method.Invoke(zipArchive, new object[0]);
                }
            }
        }
    }
}
