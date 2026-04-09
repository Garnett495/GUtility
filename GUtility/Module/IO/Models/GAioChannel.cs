using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Enums;

namespace GUtility.Module.IO.Models
{
    /// <summary>
    /// 單一 AO 通道資訊。
    /// </summary>
    public class GAioChannel
    {
        /// <summary>
        /// 通道編號。
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// 通道名稱。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 類比範圍。
        /// </summary>
        public GAnalogRangeType RangeType { get; set; }

        /// <summary>
        /// 目前值。
        /// </summary>
        public double Value { get; set; }

        public GAioChannel()
        {
            Name = string.Empty;
            RangeType = GAnalogRangeType.Voltage0To10;
        }
    }
}
