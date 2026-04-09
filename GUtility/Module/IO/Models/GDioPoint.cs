using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Enums;

namespace GUtility.Module.IO.Models
{
    /// <summary>
    /// 單一數位點位資訊。
    /// </summary>
    public class GDioPoint
    {
        /// <summary>
        /// 點位編號。
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// 點位名稱。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// I/O 方向。
        /// </summary>
        public GIoDirection Direction { get; set; }

        /// <summary>
        /// 目前值。
        /// </summary>
        public bool Value { get; set; }

        public GDioPoint()
        {
            Name = string.Empty;
        }
    }
}
