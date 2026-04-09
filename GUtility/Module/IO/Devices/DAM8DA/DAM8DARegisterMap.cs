using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.IO.Devices.DAM8DA
{
    /// <summary>
    /// DAM8DA 寄存器定義。
    /// 
    /// 注意：
    /// 這裡的位址與縮放比例請依原廠手冊確認。
    /// 目前先使用常見 8 通道 AO 與 0~10V -> 0~4000 的設計方式集中管理。
    /// </summary>
    public static class DAM8DARegisterMap
    {
        /// <summary>
        /// AO Holding Register 起始位址。
        /// </summary>
        public const ushort AoStartAddress = 0;

        /// <summary>
        /// AO 通道數量。
        /// </summary>
        public const ushort AoCount = 8;

        /// <summary>
        /// 原始最小值。
        /// </summary>
        public const ushort RawMin = 0;

        /// <summary>
        /// 原始最大值。
        /// </summary>
        public const ushort RawMax = 4000;

        /// <summary>
        /// 電壓最小值。
        /// </summary>
        public const double VoltageMin = 0.0;

        /// <summary>
        /// 電壓最大值。
        /// </summary>
        public const double VoltageMax = 10.0;
    }
}
