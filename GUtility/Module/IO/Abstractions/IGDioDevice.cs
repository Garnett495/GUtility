namespace GUtility.Module.IO.Abstractions
{
    /// <summary>
    /// 數位 I/O 裝置介面。
    /// </summary>
    public interface IGDioDevice : IGDevice
    {
        /// <summary>
        /// DI 點數。
        /// </summary>
        int DiChannelCount { get; }

        /// <summary>
        /// DO 點數。
        /// </summary>
        int DoChannelCount { get; }

        /// <summary>
        /// 讀取單一 DI。
        /// </summary>
        bool ReadDI(int channel);

        /// <summary>
        /// 讀取所有 DI。
        /// </summary>
        bool[] ReadAllDI();

        /// <summary>
        /// 讀取單一 DO 狀態。
        /// </summary>
        bool ReadDO(int channel);

        /// <summary>
        /// 讀取所有 DO 狀態。
        /// </summary>
        bool[] ReadAllDO();

        /// <summary>
        /// 寫入單一 DO。
        /// </summary>
        void WriteDO(int channel, bool value);

        /// <summary>
        /// 一次寫入所有 DO。
        /// </summary>
        void WriteAllDO(bool[] values);
    }
}