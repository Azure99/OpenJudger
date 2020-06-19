using System;

namespace Judger.Managers
{
    /// <summary>
    /// 处理器亲和性管理器
    /// </summary>
    /// 用于向判题任务分配独立处理器核心
    public static class ProcessorAffinityManager
    {
        private static readonly object Lock = new object();
        private static readonly int ProcessorCount;
        private static readonly int DefaultAffinityInt;
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
        /// 申请处理器核心
        /// </summary>
        public static IntPtr GetUsage()
        {
            lock (Lock)
            {
                int affinity = DefaultAffinityInt;
                // 遍历所有处理器核心
                for (int i = 0; i < ProcessorCount; i++)
                {
                    // 此核心未被占用则分配此核心
                    if ((_usingProcessor & (1 << i)) == 0)
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
        /// 释放Usage对应的处理器核心
        /// </summary>
        public static void ReleaseUsage(IntPtr affinity)
        {
            int affinityInt = affinity.ToInt32();
            if (affinityInt < DefaultAffinityInt)
                _usingProcessor ^= affinityInt;
        }
    }
}