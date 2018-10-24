﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Judger.Managers;
using Judger.Models;

namespace Judger.Fetcher
{
    /// <summary>
    /// 通过反射动态加载Fetcher
    /// </summary>
    public static class FetcherFactory
    {
        //Fetcher程序集
        private static Assembly _fetcherAssembly;
        private static Configuration _config = ConfigManager.Config;
        static FetcherFactory()
        {
            string dllPath = Path.GetFullPath(_config.FetcherDLLPath);
            _fetcherAssembly = Assembly.LoadFile(dllPath);
        }

        /// <summary>
        /// 动态创建TaskFetcher
        /// </summary>
        public static ITaskFetcher CreateTaskFetcher()
        {
            foreach(Type type in _fetcherAssembly.ExportedTypes)
            {
                if(type.GetInterface(typeof(ITaskFetcher).FullName) != null)
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
                if (type.GetInterface(typeof(ITaskSubmitter).FullName) != null)
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
                if (type.GetInterface(typeof(ITestDataFetcher).FullName) != null)
                {
                    return _fetcherAssembly.CreateInstance(type.FullName) as ITestDataFetcher;
                }
            }

            throw new FetcherException("ITestDataFetcher not implement!");
        }
    }
}
