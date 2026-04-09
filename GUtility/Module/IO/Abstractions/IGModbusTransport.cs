namespace GUtility.Module.IO.Abstractions
{
    /// <summary>
    /// Modbus 通訊傳輸層介面。
    /// </summary>
    public interface IGModbusTransport
    {
        /// <summary>
        /// 目前是否已開啟連線。
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 建立 TCP 連線。
        /// </summary>
        bool Connect(string ip, int port);

        /// <summary>
        /// 關閉連線。
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 讀取 Coils。
        /// </summary>
        bool[] ReadCoils(byte slaveId, ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 讀取 Discrete Inputs。
        /// </summary>
        bool[] ReadInputs(byte slaveId, ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 讀取 Holding Registers。
        /// </summary>
        ushort[] ReadHoldingRegisters(byte slaveId, ushort startAddress, ushort numberOfPoints);

        /// <summary>
        /// 寫入單一 Coil。
        /// </summary>
        void WriteSingleCoil(byte slaveId, ushort coilAddress, bool value);

        /// <summary>
        /// 寫入多個 Coils。
        /// </summary>
        void WriteMultipleCoils(byte slaveId, ushort startAddress, bool[] data);

        /// <summary>
        /// 寫入單一 Holding Register。
        /// </summary>
        void WriteSingleRegister(byte slaveId, ushort registerAddress, ushort value);

        /// <summary>
        /// 寫入多個 Holding Registers。
        /// </summary>
        void WriteMultipleRegisters(byte slaveId, ushort startAddress, ushort[] data);
    }
}