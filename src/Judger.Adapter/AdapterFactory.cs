using System;
using System.IO;
using System.Reflection;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;

namespace Judger.Adapter
{
    /// <summary>
    /// 通过反射加载Adapter
    /// </summary>
    public static class AdapterFactory
    {
        // Adapter程序集
        private static readonly Assembly AdapterAssembly;

        static AdapterFactory()
        {
            LogManager.Info("Load adapter: " + Config.AdapterDllPath);

            string dllPath = Path.GetFullPath(Config.AdapterDllPath);
            AdapterAssembly = Assembly.LoadFile(dllPath);
        }

        private static Configuration Config { get; } = ConfigManager.Config;

        public static ITaskFetcher CreateTaskFetcher()
        {
            foreach (Type type in AdapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTaskFetcher) ||
                    type.GetInterface(typeof(ITaskFetcher).FullName) != null)
                    return AdapterAssembly.CreateInstance(type.FullName) as ITaskFetcher;
            }

            throw new AdapterException("ITaskFetcher not implement!");
        }

        public static ITaskSubmitter CreateTaskSubmitter()
        {
            foreach (Type type in AdapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTaskSubmitter) ||
                    type.GetInterface(typeof(ITaskSubmitter).FullName) != null)
                    return AdapterAssembly.CreateInstance(type.FullName) as ITaskSubmitter;
            }

            throw new AdapterException("ITaskSubmitter not implement!");
        }

        public static ITestDataFetcher CreateTestDataFetcher()
        {
            foreach (Type type in AdapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTestDataFetcher) ||
                    type.GetInterface(typeof(ITestDataFetcher).FullName) != null)
                    return AdapterAssembly.CreateInstance(type.FullName) as ITestDataFetcher;
            }

            throw new AdapterException("ITestDataFetcher not implement!");
        }
    }
}