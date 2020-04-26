using System;

namespace Judger.Managers
{
    /// <summary>
    /// 处理器亲和性管理器, 用于向判题任务分配独立处理器核心
    /// </summary>
    public static class ProcessorAffinityManager
    {
        private static readonly object Lock = new object();

        /// <summary>
        /// CPU核心数
        /// </summary>
        private static readonly int ProcessorCount;

        /// <summary>
        /// 默认亲和性
        /// </summary>
        private static readonly int DefaultAffinityInt;

        /// <summary>
        /// 被使用的核心(二进制表示)
        /// </summary>
        private static int _usingProcessor;

        static ProcessorAffinityManager()
        {
            ProcessorCount = Environment.ProcessorCount;
            for (int i = 0; i < ProcessorCount; i++)
                DefaultAffinityInt += 1 << i;
        }

        /// <summary>
        /// 默认处理器亲和性
        /// </summary>
        public static IntPtr DefaultAffinity => new IntPtr(DefaultAffinityInt);

        /// <summary>
        /// 申请处理器使用权
        /// </summary>
        public static IntPtr GetUsage()
        {
            lock (Lock)
            {
                int affinity = DefaultAffinityInt;
                for (int i = 0; i < ProcessorCount; i++) // 遍历所有处理器核心
                {
                    if ((_usingProcessor & (1 << i)) == 0) // 判断此处理器核心是否被占用
                    {
                        affinity = 1 << i;
                        _usingProcessor |= affinity;
                        break;
                    }
                }

                return new IntPtr(affinity);
            }
        }

        /// <summary>
        /// 释放usage对应处理器
        /// </summary>
        public static void ReleaseUsage(IntPtr affinity)
        {
            int affinityInt = affinity.ToInt32();
            if (affinityInt < DefaultAffinityInt)
                _usingProcessor ^= affinityInt;
        }
    }
}