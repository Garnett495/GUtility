using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Recipe.Core
{
    /// <summary>
    /// Recipe 差異資訊。
    /// </summary>
    [Serializable]
    public class GRecipeDifference
    {
        /// <summary>
        /// 差異欄位路徑。
        /// 例如：Data.Exposure
        /// </summary>
        public string PropertyPath { get; set; }

        /// <summary>
        /// 左側值。
        /// </summary>
        public string LeftValue { get; set; }

        /// <summary>
        /// 右側值。
        /// </summary>
        public string RightValue { get; set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        public GRecipeDifference()
        {
            PropertyPath = string.Empty;
            LeftValue = string.Empty;
            RightValue = string.Empty;
        }

        /// <summary>
        /// 建立差異資訊。
        /// </summary>
        /// <param name="propertyPath">欄位路徑。</param>
        /// <param name="leftValue">左側值。</param>
        /// <param name="rightValue">右側值。</param>
        public GRecipeDifference(string propertyPath, string leftValue, string rightValue)
        {
            PropertyPath = propertyPath ?? string.Empty;
            LeftValue = leftValue ?? string.Empty;
            RightValue = rightValue ?? string.Empty;
        }

        /// <summary>
        /// 轉為可閱讀字串。
        /// </summary>
        /// <returns>差異描述字串。</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1} -> {2}", PropertyPath, LeftValue, RightValue);
        }
    }
}
