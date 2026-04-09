using GUtility.Module.IO.Models;

namespace GUtility.Module.IO.Abstractions
{
    /// <summary>
    /// 使用 Modbus 通訊的裝置介面。
    /// </summary>
    public interface IGModbusDevice : IGDevice
    {
        /// <summary>
        /// Modbus 相關設定。
        /// </summary>
        GModbusDeviceConfig ModbusConfig { get; }
    }
}