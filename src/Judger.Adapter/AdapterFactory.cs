using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Judger.Managers;
using Judger.Models;
using Judger.Models.Exception;

namespace Judger.Adapter
{
    /// <summary>
    /// 服务端适配器工厂
    /// </summary>
    /// 通过反射加载Adapter
    public static class AdapterFactory
    {
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
                    type.GetInterface(typeof(ITaskFetcher).FullName!) != null)
                    return AdapterAssembly.CreateInstance(type.FullName!) as ITaskFetcher;
            }

            throw new AdapterException("ITaskFetcher not implement!");
        }

        public static ITaskSubmitter CreateTaskSubmitter()
        {
            foreach (Type type in AdapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTaskSubmitter) ||
                    type.GetInterface(typeof(ITaskSubmitter).FullName!) != null)
                    return AdapterAssembly.CreateInstance(type.FullName!) as ITaskSubmitter;
            }

            throw new AdapterException("ITaskSubmitter not implement!");
        }

        public static ITestDataFetcher CreateTestDataFetcher()
        {
            foreach (Type type in AdapterAssembly.ExportedTypes)
            {
                if (type.BaseType == typeof(BaseTestDataFetcher) ||
                    type.GetInterface(typeof(ITestDataFetcher).FullName!) != null)
                    return AdapterAssembly.CreateInstance(type.FullName!) as ITestDataFetcher;
            }

            throw new AdapterException("ITestDataFetcher not implement!");
        }

        public static IConfigInitializer[] GetConfigInitializers()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] assemblyFiles =
                Directory.GetFiles(baseDir)
                    .Where(s => Path.GetFileName(s).StartsWith("Judger.Adapter"))
                    .Where(s => s.EndsWith(".dll"))
                    .ToArray();

            List<IConfigInitializer> initializers = new List<IConfigInitializer>();
            foreach (string assemblyFile in assemblyFiles)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(assemblyFile);
                    foreach (Type type in assembly.ExportedTypes)
                    {
                        if (type.GetInterface(typeof(IConfigInitializer).FullName!) != null)
                            initializers.Add(assembly.CreateInstance(type.FullName!) as IConfigInitializer);
                    }
                }
                catch (BadImageFormatException)
                { }
            }

            return initializers.ToArray();
        }
    }
}