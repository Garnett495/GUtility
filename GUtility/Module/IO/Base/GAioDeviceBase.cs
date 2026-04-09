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
    /// 類比輸出裝置（AIO）基底類別。
    /// 
    /// 設計目的：
    /// 1. 統一所有 AO 裝置的基本行為
    /// 2. 提供 Snapshot 快取機制，避免頻繁打 Modbus
    /// 3. 提供 Thread-safe 存取（lock 保護）
    /// 
    /// 子類別需實作：
    /// - WriteAO()：實際寫入設備
    /// - Refresh()（在上層呼叫）：更新 Snapshot
    /// </summary>
    public abstract class GAioDeviceBase : GModbusDeviceBase, IGAioDevice
    {
        /// <summary>
        /// 同步鎖，用於保護 Snapshot 資料在多執行緒下的安全性。
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// AO 快取資料（Snapshot）。
        /// 
        /// 說明：
        /// - 所有 ReadAO / ReadAllAO 都從這裡讀
        /// - 實際設備資料需透過 Refresh() 更新
        /// </summary>
        protected GIoSnapshot Snapshot { get; private set; }

        /// <summary>
        /// AO 通道數量。
        /// 由子類別（例如 DAM8DA）在建構時設定。
        /// </summary>
        public int AoChannelCount { get; protected set; }

        /// <summary>
        /// 類比輸出範圍（例如 0~10V / 4~20mA）。
        /// </summary>
        public GAnalogRangeType AnalogRange { get; protected set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        /// <param name="config">Modbus 設定</param>
        /// <param name="transport">Modbus 通訊介面</param>
        protected GAioDeviceBase(GModbusDeviceConfig config, IGModbusTransport transport)
            : base(config, transport)
        {
            // 初始化 Snapshot
            Snapshot = new GIoSnapshot();

            // 預設類比範圍為 0~10V
            AnalogRange = GAnalogRangeType.Voltage0To10;
        }

        /// <summary>
        /// 更新 AO 快取資料。
        /// 
        /// 通常由子類別在 Refresh() 中呼叫。
        /// </summary>
        /// <param name="values">AO 值陣列</param>
        protected void SetAoSnapshot(double[] values)
        {
            lock (_syncRoot)
            {
                // 若為 null，避免 NullReference
                Snapshot.AO = values ?? new double[0];

                // 更新時間戳記
                Snapshot.UpdateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 讀取單一 AO（從快取）。
        /// 
        /// 注意：
        /// - 不會直接讀設備
        /// - 資料來源為 Snapshot
        /// </summary>
        /// <param name="channel">通道編號（0-based）</param>
        /// <returns>AO 值</returns>
        public virtual double ReadAO(int channel)
        {
            // 檢查 channel 合法性
            ValidateChannel(channel, AoChannelCount);

            lock (_syncRoot)
            {
                return Snapshot.AO[channel];
            }
        }

        /// <summary>
        /// 讀取所有 AO（從快取）。
        /// 
        /// 注意：
        /// - 回傳 Clone，避免外部修改內部資料
        /// </summary>
        /// <returns>AO 陣列</returns>
        public virtual double[] ReadAllAO()
        {
            lock (_syncRoot)
            {
                return (double[])Snapshot.AO.Clone();
            }
        }

        /// <summary>
        /// 寫入單一 AO。
        /// 
        /// 子類別必須實作：
        /// - Modbus 寫入邏輯
        /// - 電壓/電流轉換
        /// - Snapshot 更新
        /// </summary>
        /// <param name="channel">通道編號</param>
        /// <param name="value">欲寫入的值（例如電壓）</param>
        public abstract void WriteAO(int channel, double value);

        /// <summary>
        /// 驗證通道是否在合法範圍內。
        /// </summary>
        /// <param name="channel">通道編號</param>
        /// <param name="maxCount">最大通道數</param>
        protected void ValidateChannel(int channel, int maxCount)
        {
            if (channel < 0 || channel >= maxCount)
            {
                throw new ArgumentOutOfRangeException("channel",
                    "AO channel out of range: " + channel.ToString());
            }
        }
    }
}