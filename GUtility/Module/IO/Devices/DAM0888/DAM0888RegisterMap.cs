using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.IO.Devices.DAM0888
{
    /// <summary>
    /// DAM0888 寄存器定義。
    /// 
    /// 注意：
    /// 這裡的位址請依你手上的原廠手冊做最後確認。
    /// 本類別的目的，是將設備位址集中管理，避免散落在程式各處。
    /// </summary>
    public static class DAM0888RegisterMap
    {
        /// <summary>
        /// DI 起始位址。
        /// </summary>
        public const ushort DiStartAddress = 0;

        /// <summary>
        /// DI 點數。
        /// </summary>
        public const ushort DiCount = 8;

        /// <summary>
        /// DO 起始 Coil 位址。
        /// </summary>
        public const ushort DoStartAddress = 0;

        /// <summary>
        /// DO 點數。
        /// </summary>
        public const ushort DoCount = 8;
    }
}
