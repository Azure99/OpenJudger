using System;
using System.Collections.Generic;
using Judger.Models.Database;
using Judger.Models.Program;

namespace Judger.Models
{
    /// <summary>
    /// 配置信息
    /// </summary>
    [Serializable]
    public class Configuration
    {
        /// <summary>
        /// 启动时加载的Adapter DLL路径
        /// </summary>
        public string AdapterDllPath { get; set; } = "Judger.Adapter.Generic.dll";

        /// <summary>
        /// 评测机名称
        /// </summary>
        public string JudgerName { get; set; } = "OpenJudger";

        /// <summary>
        /// 评测机密码
        /// </summary>
        public string Password { get; set; } = "RainngAzure99";

        /// <summary>
        /// 取回任务周期
        /// </summary>
        public int TaskFetchInterval { get; set; } = 1500;

        /// <summary>
        /// (取回/提交)任务超时时间
        /// </summary>
        public int FetchTimeout { get; set; } = 5000;

        /// <summary>
        /// 取回判题任务接口URL
        /// </summary>
        public string TaskFetchUrl { get; set; } = "http://localhost/judger/fetchtask";

        /// <summary>
        /// 取回测试数据接口URL
        /// </summary>
        public string TestDataFetchUrl { get; set; } = "http://localhost/judger/fetchdata";

        /// <summary>
        /// 提交判题结果接口URL
        /// </summary>
        public string ResultSubmitUrl { get; set; } = "http://localhost/judger/submitresult";

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
        /// 是否拦截不安全代码
        /// </summary>
        public bool InterceptUnsafeCode { get; set; } = true;

        /// <summary>
        /// 拦截规则
        /// </summary>
        public string InterceptionRules { get; set; } = "InterceptionRules.txt";

        /// <summary>
        /// 时间、内存消耗 监控周期(ms)
        /// </summary>
        public int MonitorInterval { get; set; } = 10;

        /// <summary>
        /// 最小内存消耗值, 如果获取到的内存消耗为0则返回此值
        /// </summary>
        public int MinimumMemoryCost { get; set; } = 256;

        /// <summary>
        /// 附加设置
        /// </summary>
        public Dictionary<string, string> AdditionalConfigs { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 语言(编译/运行)配置
        /// </summary>
        public ProgramLangConfig[] Languages { get; set; } =
            {new ProgramLangConfig()};

        /// <summary>
        /// 数据库配置
        /// </summary>
        public DbLangConfig[] Databases { get; set; } =
            {new DbLangConfig()};
    }
}