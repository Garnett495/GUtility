using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Abstractions;
using GUtility.Module.IO.Models;

namespace GUtility.Module.IO.Base
{
    /// <summary>
    /// 數位 I/O 裝置（DIO）基底類別。
    /// 
    /// 設計目的：
    /// 1. 統一所有數位 I/O 設備的基本行為（DI / DO）
    /// 2. 提供 Snapshot 快取機制（避免頻繁通訊）
    /// 3. 提供 Thread-safe 存取（lock 保護）
    /// 4. 將設備差異（Register / 通訊）交由子類別實作
    /// 
    /// 架構分工：
    /// - Base（本類）：負責 cache、驗證、thread-safe
    /// - Device（例如 DAM0888）：負責 Modbus 操作與 mapping
    /// 
    /// 使用原則：
    /// - ReadDI / ReadDO → 讀取快取（不直接打設備）
    /// - Refresh()（由上層呼叫）→ 更新 Snapshot
    /// </summary>
    public abstract class GDioDeviceBase : GModbusDeviceBase, IGDioDevice
    {
        /// <summary>
        /// 同步鎖，用於保護 Snapshot 在多執行緒下的安全性。
        /// </summary>
        private readonly object _syncRoot = new object();

        /// <summary>
        /// I/O 快取資料（Snapshot）。
        /// 
        /// 說明：
        /// - DI / DO 狀態皆儲存在此
        /// - 所有讀取操作皆從此快取取得
        /// </summary>
        protected GIoSnapshot Snapshot { get; private set; }

        /// <summary>
        /// DI（Digital Input）通道數量。
        /// 由子類別設定（例如 8 / 16 / 32）。
        /// </summary>
        public int DiChannelCount { get; protected set; }

        /// <summary>
        /// DO（Digital Output）通道數量。
        /// </summary>
        public int DoChannelCount { get; protected set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        /// <param name="config">Modbus 設定</param>
        /// <param name="transport">Modbus 通訊介面</param>
        protected GDioDeviceBase(GModbusDeviceConfig config, IGModbusTransport transport)
            : base(config, transport)
        {
            // 初始化 Snapshot
            Snapshot = new GIoSnapshot();
        }

        /// <summary>
        /// 更新 DI 快取資料。
        /// 
        /// 通常由子類別在 Refresh() 中呼叫。
        /// </summary>
        /// <param name="values">DI 陣列</param>
        protected void SetDiSnapshot(bool[] values)
        {
            lock (_syncRoot)
            {
                Snapshot.DI = values ?? new bool[0];
                Snapshot.UpdateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 更新 DO 快取資料。
        /// </summary>
        /// <param name="values">DO 陣列</param>
        protected void SetDoSnapshot(bool[] values)
        {
            lock (_syncRoot)
            {
                Snapshot.DO = values ?? new bool[0];
                Snapshot.UpdateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 讀取單一 DI（從快取）。
        /// 
        /// 注意：
        /// - 不會直接讀取設備
        /// - 需先透過 Refresh() 更新資料
        /// </summary>
        /// <param name="channel">通道編號（0-based）</param>
        /// <returns>DI 狀態</returns>
        public virtual bool ReadDI(int channel)
        {
            ValidateChannel(channel, DiChannelCount, "DI");

            lock (_syncRoot)
            {
                return Snapshot.DI[channel];
            }
        }

        /// <summary>
        /// 讀取所有 DI（從快取）。
        /// 
        /// 回傳 Clone，避免外部修改內部資料。
        /// </summary>
        /// <returns>DI 陣列</returns>
        public virtual bool[] ReadAllDI()
        {
            lock (_syncRoot)
            {
                return (bool[])Snapshot.DI.Clone();
            }
        }

        /// <summary>
        /// 讀取單一 DO（從快取）。
        /// </summary>
        /// <param name="channel">通道編號</param>
        /// <returns>DO 狀態</returns>
        public virtual bool ReadDO(int channel)
        {
            ValidateChannel(channel, DoChannelCount, "DO");

            lock (_syncRoot)
            {
                return Snapshot.DO[channel];
            }
        }

        /// <summary>
        /// 讀取所有 DO（從快取）。
        /// </summary>
        /// <returns>DO 陣列</returns>
        public virtual bool[] ReadAllDO()
        {
            lock (_syncRoot)
            {
                return (bool[])Snapshot.DO.Clone();
            }
        }

        /// <summary>
        /// 寫入單一 DO。
        /// 
        /// 子類別必須實作：
        /// - 實際 Modbus 寫入
        /// - Snapshot 更新
        /// - 例外處理
        /// </summary>
        /// <param name="channel">通道編號</param>
        /// <param name="value">輸出值</param>
        public abstract void WriteDO(int channel, bool value);

        /// <summary>
        /// 一次寫入所有 DO。
        /// 
        /// 子類別必須實作：
        /// - 批次寫入 Modbus
        /// - Snapshot 更新
        /// </summary>
        /// <param name="values">DO 陣列</param>
        public abstract void WriteAllDO(bool[] values);

        /// <summary>
        /// 驗證通道是否在合法範圍內。
        /// </summary>
        /// <param name="channel">通道編號</param>
        /// <param name="maxCount">最大通道數</param>
        /// <param name="category">類型（DI / DO）</param>
        protected void ValidateChannel(int channel, int maxCount, string category)
        {
            if (channel < 0 || channel >= maxCount)
            {
                throw new ArgumentOutOfRangeException("channel",
                    category + " channel out of range: " + channel.ToString());
            }
        }
    }
}