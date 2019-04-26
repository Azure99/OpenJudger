using System;
using System.IO;
using System.Reflection;
using Judger.Entity;
using Judger.Entity.Exception;
using Judger.Managers;

namespace Judger.Fetcher
{
    /// <summary>
    /// 通过反射动态加载Fetcher
    /// </summary>
    public static class FetcherFactory
    {
        /// <summary>
        /// Fetcher程序集
        /// </summary>
        private static Assembly _fetcherAssembly;
        private static Configuration Config { get; } = ConfigManager.Config;

        static FetcherFactory()
        {
            LogManager.Info("Load fetcher: " + Config.FetcherDllPath);

            string dllPath = Path.GetFullPath(Config.FetcherDllPath);
            _fetcherAssembly = Assembly.LoadFile(dllPath);
        }

        /// <summary>
        /// 动态创建TaskFetcher
        /// </summary>
        public static ITaskFetcher CreateTaskFetcher()
        {
            foreach (Type type in _fetcherAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTaskFetcher) ||
                    type.GetInterface(typeof(ITaskFetcher).FullName) != null)
                {
                    return _fetcherAssembly.CreateInstance(type.FullName) as ITaskFetcher;
                }
            }

            throw new FetcherException("ITaskFetcher not implement!");
        }

        /// <summary>
        /// 动态创建TaskSubmitter
        /// </summary>
        public static ITaskSubmitter CreateTaskSubmitter()
        {
            foreach (Type type in _fetcherAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTaskSubmitter) ||
                    type.GetInterface(typeof(ITaskSubmitter).FullName) != null)
                {
                    return _fetcherAssembly.CreateInstance(type.FullName) as ITaskSubmitter;
                }
            }

            throw new FetcherException("ITaskSubmitter not implement!");
        }

        /// <summary>
        /// 动态创建TestDataFetcher
        /// </summary>
        public static ITestDataFetcher CreateTestDataFetcher()
        {
            foreach (Type type in _fetcherAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTestDataFetcher) ||
                    type.GetInterface(typeof(ITestDataFetcher).FullName) != null)
                {
                    return _fetcherAssembly.CreateInstance(type.FullName) as ITestDataFetcher;
                }
            }

            throw new FetcherException("ITestDataFetcher not implement!");
        }
    }
}