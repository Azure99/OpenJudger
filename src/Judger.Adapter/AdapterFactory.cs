using System;
using System.IO;
using System.Reflection;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;

namespace Judger.Adapter
{
    /// <summary>
    /// 通过反射动态加载Adapter
    /// </summary>
    public static class AdapterFactory
    {
        /// <summary>
        /// Adapter程序集
        /// </summary>
        private static readonly Assembly _adapterAssembly;

        static AdapterFactory()
        {
            LogManager.Info("Load adapter: " + Config.AdapterDllPath);

            string dllPath = Path.GetFullPath(Config.AdapterDllPath);
            _adapterAssembly = Assembly.LoadFile(dllPath);
        }

        private static Configuration Config { get; } = ConfigManager.Config;

        /// <summary>
        /// 动态创建TaskFetcher
        /// </summary>
        public static ITaskFetcher CreateTaskFetcher()
        {
            foreach (Type type in _adapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTaskFetcher) ||
                    type.GetInterface(typeof(ITaskFetcher).FullName) != null)
                    return _adapterAssembly.CreateInstance(type.FullName) as ITaskFetcher;
            }

            throw new AdapterException("ITaskFetcher not implement!");
        }

        /// <summary>
        /// 动态创建TaskSubmitter
        /// </summary>
        public static ITaskSubmitter CreateTaskSubmitter()
        {
            foreach (Type type in _adapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTaskSubmitter) ||
                    type.GetInterface(typeof(ITaskSubmitter).FullName) != null)
                    return _adapterAssembly.CreateInstance(type.FullName) as ITaskSubmitter;
            }

            throw new AdapterException("ITaskSubmitter not implement!");
        }

        /// <summary>
        /// 动态创建TestDataFetcher
        /// </summary>
        public static ITestDataFetcher CreateTestDataFetcher()
        {
            foreach (Type type in _adapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTestDataFetcher) ||
                    type.GetInterface(typeof(ITestDataFetcher).FullName) != null)
                    return _adapterAssembly.CreateInstance(type.FullName) as ITestDataFetcher;
            }

            throw new AdapterException("ITestDataFetcher not implement!");
        }
    }
}