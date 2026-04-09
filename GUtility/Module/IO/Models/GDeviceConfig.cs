using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Enums;

namespace GUtility.Module.IO.Models
{
    /// <summary>
    /// 一般裝置設定。
    /// </summary>
    public class GDeviceConfig
    {
        /// <summary>
        /// 裝置名稱。
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 裝置類型。
        /// </summary>
        public GDeviceType DeviceType { get; set; }

        /// <summary>
        /// 輪詢間隔時間（毫秒）。
        /// </summary>
        public int PollingIntervalMs { get; set; }

        public GDeviceConfig()
        {
            DeviceName = string.Empty;
            DeviceType = GDeviceType.Unknown;
            PollingIntervalMs = 200;
        }
    }
}