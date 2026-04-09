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
    /// Modbus 裝置共用基底類別。
    /// 
    /// 設計目的：
    /// 1. 封裝所有 Modbus 設備的共通行為（Connect / Disconnect）
    /// 2. 將通訊層（Transport）與設備邏輯分離
    /// 3. 統一設備初始化與設定來源（ModbusConfig）
    /// 4. 提供子類別（DIO / AIO）共用基礎
    /// 
    /// 架構分層：
    /// - GDeviceBase         → 裝置生命週期
    /// - GModbusDeviceBase   → Modbus 通訊管理（本類）
    /// - GDio / GAio Device  → 實際設備邏輯
    /// 
    /// 子類別責任：
    /// - 實作 Refresh()
    /// - 實作實際 I/O 操作（讀寫 register / coil）
    /// </summary>
    public abstract class GModbusDeviceBase : GDeviceBase, IGModbusDevice
    {
        /// <summary>
        /// Modbus 裝置設定（IP / Port / SlaveId 等）。
        /// </summary>
        public GModbusDeviceConfig ModbusConfig { get; protected set; }

        /// <summary>
        /// Modbus 通訊傳輸層（例如 TCP）。
        /// 
        /// 說明：
        /// - 封裝所有通訊細節（Socket / 封包）
        /// - Device 不直接操作底層通訊
        /// </summary>
        protected IGModbusTransport Transport { get; private set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        /// <param name="config">Modbus 設定</param>
        /// <param name="transport">通訊層實作（例如 ModbusTcpTransport）</param>
        protected GModbusDeviceBase(GModbusDeviceConfig config, IGModbusTransport transport)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (transport == null) throw new ArgumentNullException("transport");

            ModbusConfig = config;
            Transport = transport;

            // 初始化裝置基本資訊
            DeviceName = config.DeviceName;
            DeviceType = config.DeviceType;
        }

        /// <summary>
        /// 建立 Modbus 連線。
        /// 
        /// 流程：
        /// 1. 設定狀態為 Connecting
        /// 2. 呼叫 Transport.Connect()
        /// 3. 根據結果更新 ConnectionState
        /// 4. 更新 Status
        /// </summary>
        /// <returns>成功回傳 true，失敗回傳 false</returns>
        public override bool Connect()
        {
            try
            {
                ConnectionState = GDeviceConnectionState.Connecting;
                UpdateStatus(string.Empty);

                bool result = Transport.Connect(ModbusConfig.IP, ModbusConfig.Port);

                ConnectionState = result
                    ? GDeviceConnectionState.Connected
                    : GDeviceConnectionState.Error;

                UpdateStatus(result ? string.Empty : "Connect failed.");
                return result;
            }
            catch (Exception ex)
            {
                // 發生例外時，標記為 Error
                ConnectionState = GDeviceConnectionState.Error;
                UpdateStatus(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 中斷 Modbus 連線。
        /// 
        /// 流程：
        /// 1. 呼叫 Transport.Disconnect()
        /// 2. 更新 ConnectionState
        /// 3. 更新 Status
        /// </summary>
        public override void Disconnect()
        {
            try
            {
                Transport.Disconnect();

                ConnectionState = GDeviceConnectionState.Disconnected;
                UpdateStatus(string.Empty);
            }
            catch (Exception ex)
            {
                // Disconnect 發生錯誤仍需記錄
                ConnectionState = GDeviceConnectionState.Error;
                UpdateStatus(ex.Message);
            }
        }

        /// <summary>
        /// 釋放 Transport 資源。
        /// 
        /// 說明：
        /// - 若 Transport 有實作 IDisposable，則釋放
        /// - 避免 Socket / Network 資源洩漏
        /// </summary>
        protected override void DisposeManagedResources()
        {
            var disposable = Transport as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}