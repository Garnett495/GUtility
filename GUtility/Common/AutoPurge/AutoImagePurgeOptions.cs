using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Storage
{
    /// <summary>
    /// 自動清除影像檔案設定。
    /// </summary>
    public class AutoImagePurgeOptions
    {
        /// <summary>
        /// 要清除的目標資料夾。
        /// </summary>
        public string TargetFolder { get; set; }

        /// <summary>
        /// 磁碟使用率達到此百分比時開始清理。
        /// 例如 80 表示 80%。
        /// </summary>
        public int StartPurgeUsedPercent { get; set; }

        /// <summary>
        /// 清理到磁碟使用率低於此百分比時停止。
        /// 例如 70 表示 70%。
        /// </summary>
        public int StopPurgeUsedPercent { get; set; }

        /// <summary>
        /// 檢查週期（秒）。
        /// </summary>
        public int CheckIntervalSeconds { get; set; }

        /// <summary>
        /// 是否包含子資料夾。
        /// </summary>
        public bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// 要掃描的副檔名 Pattern，例如 *.bmp / *.jpg / *.png。
        /// </summary>
        public List<string> SearchPatterns { get; set; }

        /// <summary>
        /// 檔案至少存在多久以上才允許刪除，避免刪到剛存進去的檔案。
        /// </summary>
        public int MinimumFileAgeSeconds { get; set; }

        public AutoImagePurgeOptions()
        {
            StartPurgeUsedPercent = 80;
            StopPurgeUsedPercent = 70;
            CheckIntervalSeconds = 30;
            IncludeSubdirectories = true;
            MinimumFileAgeSeconds = 30;

            SearchPatterns = new List<string>
            {
                "*.bmp",
                "*.jpg",
                "*.jpeg",
                "*.png",
                "*.tif",
                "*.tiff"
            };
        }
    }
}
