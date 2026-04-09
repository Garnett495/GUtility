using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Recipe.Core
{
    /// <summary>
    /// Recipe 比較選項。
    /// </summary>
    [Serializable]
    public class GCompareOptions
    {
        /// <summary>
        /// 是否忽略中繼資訊欄位。
        /// 例如：RecipeName、Format、CreatedTime、ModifiedTime、SchemaVersion。
        /// </summary>
        public bool IgnoreMetadata { get; set; }

        /// <summary>
        /// 比較字串時是否忽略大小寫。
        /// </summary>
        public bool IgnoreStringCase { get; set; }

        /// <summary>
        /// 是否將 null 與空字串視為相同。
        /// </summary>
        public bool TreatNullAndEmptyStringAsEqual { get; set; }

        /// <summary>
        /// 是否啟用遞迴比較巢狀物件。
        /// </summary>
        public bool RecursiveCompare { get; set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        public GCompareOptions()
        {
            IgnoreMetadata = true;
            IgnoreStringCase = false;
            TreatNullAndEmptyStringAsEqual = false;
            RecursiveCompare = true;
        }

        /// <summary>
        /// 建立預設比較選項。
        /// </summary>
        /// <returns>預設比較選項。</returns>
        public static GCompareOptions Default()
        {
            return new GCompareOptions();
        }
    }
}
