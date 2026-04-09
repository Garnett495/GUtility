using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Enums;

namespace GUtility.Module.IO.Models
{
    /// <summary>
    /// 裝置目前狀態資訊。
    /// </summary>
    public class GDeviceStatus
    {
        /// <summary>
        /// 裝置名稱。
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 是否已連線。
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// 連線狀態。
        /// </summary>
        public GDeviceConnectionState ConnectionState { get; set; }

        /// <summary>
        /// 最後更新時間。
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 最後錯誤訊息。
        /// </summary>
        public string LastErrorMessage { get; set; }

        public GDeviceStatus()
        {
            DeviceName = string.Empty;
            IsConnected = false;
            ConnectionState = GDeviceConnectionState.Disconnected;
            LastUpdateTime = DateTime.MinValue;
            LastErrorMessage = string.Empty;
        }
    }
}
