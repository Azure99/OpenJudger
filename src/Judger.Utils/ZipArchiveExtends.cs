using System.IO.Compression;
using System.Reflection;

namespace Judger.Utils
{
    /// <summary>
    /// ZipArchive扩展
    /// </summary>
    public static class ZipArchiveExtends
    {
        private const string ConstMethodWriteFile = "WriteFile";

        /// <summary>
        /// 通过反射强制将更改更新基础流
        /// </summary>
        public static void UpdateBaseStream(this ZipArchive zipArchive)
        {
            foreach (MethodInfo method in zipArchive.GetType().GetRuntimeMethods())
            {
                if (method.Name == ConstMethodWriteFile)
                {
                    method.Invoke(zipArchive, new object[0]);
                    break;
                }
            }
        }
    }
}