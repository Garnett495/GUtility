using GUtility.Module.IO.Enums;

namespace GUtility.Module.IO.Abstractions
{
    /// <summary>
    /// 類比輸出裝置介面。
    /// </summary>
    public interface IGAioDevice : IGDevice
    {
        /// <summary>
        /// AO 通道數量。
        /// </summary>
        int AoChannelCount { get; }

        /// <summary>
        /// 類比範圍設定。
        /// </summary>
        GAnalogRangeType AnalogRange { get; }

        /// <summary>
        /// 讀取單一 AO 的快取值。
        /// </summary>
        double ReadAO(int channel);

        /// <summary>
        /// 讀取所有 AO 的快取值。
        /// </summary>
        double[] ReadAllAO();

        /// <summary>
        /// 寫入單一 AO。
        /// </summary>
        void WriteAO(int channel, double value);
    }
}