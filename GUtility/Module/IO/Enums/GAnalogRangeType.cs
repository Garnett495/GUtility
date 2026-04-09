using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.IO.Enums
{
    /// <summary>
    /// 類比輸出範圍型別。
    /// </summary>
    public enum GAnalogRangeType
    {
        /// <summary>
        /// 0~10V。
        /// </summary>
        Voltage0To10 = 0,

        /// <summary>
        /// 4~20mA。
        /// </summary>
        Current4To20mA = 1
    }
}