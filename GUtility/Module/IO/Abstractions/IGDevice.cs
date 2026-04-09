using GUtility.Module.IO.Enums;
using GUtility.Module.IO.Models;
using System;

namespace GUtility.Module.IO.Abstractions
{
    /// <summary>
    /// 所有 I/O 裝置的最基本介面。
    /// </summary>
    public interface IGDevice : IDisposable
    {
        /// <summary>
        /// 裝置名稱。
        /// </summary>
        string DeviceName { get; }

        /// <summary>
        /// 裝置類型。
        /// </summary>
        GDeviceType DeviceType { get; }

        /// <summary>
        /// 目前是否已連線。
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 目前連線狀態。
        /// </summary>
        GDeviceConnectionState ConnectionState { get; }

        /// <summary>
        /// 裝置狀態資訊。
        /// </summary>
        GDeviceStatus Status { get; }

        /// <summary>
        /// 連線至裝置。
        /// </summary>
        /// <returns>成功回傳 true，失敗回傳 false。</returns>
        bool Connect();

        /// <summary>
        /// 中斷裝置連線。
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 重新整理裝置快取資料。
        /// </summary>
        void Refresh();
    }
}