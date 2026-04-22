using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Ini.Models
{
    /// <summary>
    /// Ini 檔操作選項。
    /// </summary>
    public class GIniOptions
    {
        /// <summary>
        /// 建構時若檔案不存在，是否自動建立空檔案。
        /// </summary>
        public bool AutoCreateIfMissing { get; set; }

        /// <summary>
        /// 每次 Write 後是否自動儲存到硬碟。
        /// </summary>
        public bool AutoSave { get; set; }

        /// <summary>
        /// 是否忽略 Section 大小寫。
        /// </summary>
        public bool IgnoreSectionCase { get; set; }

        /// <summary>
        /// 是否忽略 Key 大小寫。
        /// </summary>
        public bool IgnoreKeyCase { get; set; }

        public GIniOptions()
        {
            AutoCreateIfMissing = true;
            AutoSave = false;
            IgnoreSectionCase = true;
            IgnoreKeyCase = true;
        }
    }
}
