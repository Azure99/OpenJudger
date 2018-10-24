﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Judger.Models
{
    /// <summary>
    /// 配置信息
    /// </summary>
    [Serializable]
    public class Configuration
    {
        /// <summary>
        /// 通过反射加载的FetcherDLL路径
        /// </summary>
        public string FetcherDLLPath { get; set; } = "Judger.Fetcher.Generic.dll";

        /// <summary>
        /// 评测机名称
        /// </summary>
        public string JudgerName { get; set; } = "DefaultJudger";

        /// <summary>
        /// 服务器校验密码
        /// </summary>
        public string Password { get; set; } = "RainngAzure99AjUnawWEsK";

        /// <summary>
        /// 取回任务周期
        /// </summary>
        public int TaskFetchInterval { get; set; } = 1000;

        /// <summary>
        /// (取回/提交)任务超时时间
        /// </summary>
        public int FetchTimeout { get; set; } = 5000;

        /// <summary>
        /// 取回判题任务接口URL
        /// </summary>
        public string TaskFetchUrl { get; set; } = "http://localhost/judgerapi/fetchtask";

        /// <summary>
        /// 取回测试数据接口URL
        /// </summary>
        public string TestDataFetchUrl { get; set; } = "http://localhost/judgerapi/fetchdata";

        /// <summary>
        /// 提交判题结果接口URL
        /// </summary>
        public string ResultSubmitUrl { get; set; } = "http://localhost/judgerapi/submit";

        /// <summary>
        /// 测试数据存放目录
        /// </summary>
        public string TestDataDirectory { get; set; } = "TestData";

        /// <summary>
        /// 日志存放目录
        /// </summary>
        public string LogDirectory { get; set; } = "Log";

        /// <summary>
        /// 最大等待队列任务数
        /// </summary>
        public int MaxQueueSize { get; set; } = Math.Max(Environment.ProcessorCount - 2, 1);

        /// <summary>
        /// 最大同时运行的任务数
        /// </summary>
        public int MaxRunning { get; set; } = Math.Max(Environment.ProcessorCount - 2, 1);

        /// <summary>
        /// 拦截不安全代码
        /// </summary>
        public bool InterceptUnsafeCode { get; set; } = true;

        /// <summary>
        /// 拦截规则
        /// </summary>
        public string InterceptionRules { get; set; } = "InterceptionRules.txt";

        /// <summary>
        /// 附加设置
        /// </summary>
        public Dictionary<string, string> AdditionalConfig { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 编译器配置
        /// </summary>
        public LanguageConfiguration[] Languages { get; set; } = 
            new LanguageConfiguration[]{new LanguageConfiguration() };
    }
}
