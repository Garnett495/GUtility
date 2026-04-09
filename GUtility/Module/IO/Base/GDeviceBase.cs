using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Abstractions;
using GUtility.Module.IO.Enums;
using GUtility.Module.IO.Models;

namespace GUtility.Module.IO.Base
{
    /// <summary>
    /// 所有 I/O 裝置的基底類別（最上層抽象）。
    /// 
    /// 設計目的：
    /// 1. 統一所有裝置的基本屬性（名稱、狀態、類型）
    /// 2. 統一連線生命週期（Connect / Disconnect / Dispose）
    /// 3. 提供狀態追蹤機制（Status）
    /// 4. 作為 DIO / AIO / Modbus 等裝置的共同基礎
    /// 
    /// 使用方式：
    /// - 所有設備（Camera / IO / Motor）都應繼承此類別
    /// - 子類別負責實作實際行為（連線、通訊、刷新）
    /// </summary>
    public abstract class GDeviceBase : IGDevice
    {
        /// <summary>
        /// 是否已經釋放資源（避免重複 Dispose）。
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 裝置名稱（唯一識別）。
        /// 
        /// 建議：
        /// - 在 Manager 中作為 key 使用
        /// - 例如："DAM0888_1"
        /// </summary>
        public string DeviceName { get; protected set; }

        /// <summary>
        /// 裝置類型（DIO / AIO / Camera 等）。
        /// </summary>
        public GDeviceType DeviceType { get; protected set; }

        /// <summary>
        /// 是否已成功連線。
        /// 
        /// 說明：
        /// - 僅當 ConnectionState == Connected 時為 true
        /// </summary>
        public bool IsConnected
        {
            get { return ConnectionState == GDeviceConnectionState.Connected; }
        }

        /// <summary>
        /// 目前連線狀態。
        /// 
        /// 狀態流程：
        /// Disconnected → Connecting → Connected / Error
        /// </summary>
        public GDeviceConnectionState ConnectionState { get; protected set; }

        /// <summary>
        /// 裝置狀態資訊（提供 UI / Log 使用）。
        /// </summary>
        public GDeviceStatus Status { get; protected set; }

        /// <summary>
        /// 建構子，初始化基本狀態。
        /// </summary>
        protected GDeviceBase()
        {
            DeviceName = string.Empty;
            DeviceType = GDeviceType.Unknown;
            ConnectionState = GDeviceConnectionState.Disconnected;
            Status = new GDeviceStatus();
        }

        /// <summary>
        /// 更新裝置狀態資訊。
        /// 
        /// 建議使用時機：
        /// - Connect 成功 / 失敗
        /// - Refresh 成功 / 發生錯誤
        /// - 任一通訊異常
        /// </summary>
        /// <param name="errorMessage">錯誤訊息（無錯誤可傳 null）</param>
        protected void UpdateStatus(string errorMessage)
        {
            Status.DeviceName = DeviceName;
            Status.IsConnected = IsConnected;
            Status.ConnectionState = ConnectionState;
            Status.LastUpdateTime = DateTime.Now;
            Status.LastErrorMessage = errorMessage ?? string.Empty;
        }

        /// <summary>
        /// 建立裝置連線。
        /// 
        /// 子類別需實作：
        /// - TCP / Serial / SDK 初始化
        /// - ConnectionState 更新
        /// </summary>
        /// <returns>成功回傳 true，失敗回傳 false</returns>
        public abstract bool Connect();

        /// <summary>
        /// 中斷裝置連線。
        /// 
        /// 子類別需實作：
        /// - 關閉通訊
        /// - 清理資源
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// 重新整理裝置資料。
        /// 
        /// 設計用途：
        /// - 由 Manager 定時呼叫（Polling）
        /// - 更新 Snapshot / 狀態
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// 釋放裝置資源（IDisposable 實作）。
        /// 
        /// 流程：
        /// 1. Disconnect
        /// 2. 釋放子類別資源
        /// </summary>
        public void Dispose()
        {
            // 避免重複釋放
            if (_disposed) return;

            // 中斷連線
            Disconnect();

            // 讓子類別釋放額外資源
            DisposeManagedResources();

            _disposed = true;

            // 告知 GC 不需要再呼叫 Finalizer
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 提供子類別釋放 managed 資源的擴充點。
        /// 
        /// 範例：
        /// - 關閉 TcpClient
        /// - Dispose SDK instance
        /// - 清除 buffer
        /// </summary>
        protected virtual void DisposeManagedResources()
        {
        }
    }
}