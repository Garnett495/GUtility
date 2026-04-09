using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.IO.Models
{
    /// <summary>
    /// I/O 快照資料，用於快取最新狀態。
    /// </summary>
    public class GIoSnapshot
    {
        /// <summary>
        /// 最後更新時間。
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// DI 快照。
        /// </summary>
        public bool[] DI { get; set; }

        /// <summary>
        /// DO 快照。
        /// </summary>
        public bool[] DO { get; set; }

        /// <summary>
        /// AO 快照。
        /// </summary>
        public double[] AO { get; set; }

        public GIoSnapshot()
        {
            UpdateTime = DateTime.MinValue;
            DI = new bool[0];
            DO = new bool[0];
            AO = new double[0];
        }
    }
}
