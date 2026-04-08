using System;

namespace GUtility.Log
{
    /// <summary>
    /// Log 系統設定。
    /// </summary>
    public class GLogConfig
    {
        /// <summary>
        /// Log 根目錄。
        /// 例如：Logs
        /// </summary>
        public string RootFolder { get; set; }

        /// <summary>
        /// 最低輸出等級。
        /// 小於此等級的 Log 不會寫入。
        /// </summary>
        public GLogLevel MinimumLevel { get; set; }

        /// <summary>
        /// 保留天數。
        /// 超過天數的資料夾會被清除。
        /// 設為 0 或負數表示不自動清除。
        /// </summary>
        public int RetentionDays { get; set; }

        /// <summary>
        /// 單一 Queue 最大容量，避免極端情況下記憶體持續膨脹。
        /// </summary>
        public int MaxQueueCount { get; set; }

        /// <summary>
        /// 預設檔名。
        /// 例如：System.log.txt
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 建構子，設定預設值。
        /// </summary>
        public GLogConfig()
        {
            RootFolder = "Logs";
            MinimumLevel = GLogLevel.Debug;
            RetentionDays = 30;
            MaxQueueCount = 10000;
            FileName = "System.log.txt";
        }
    }
}