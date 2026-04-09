using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Recipe.Upgrade
{
    /// <summary>
    /// Recipe 升版資訊。
    /// </summary>
    [Serializable]
    public class GRecipeUpgradeContext
    {
        /// <summary>
        /// 原始版本。
        /// </summary>
        public int OriginalVersion { get; set; }

        /// <summary>
        /// 目標版本。
        /// </summary>
        public int TargetVersion { get; set; }

        /// <summary>
        /// 是否有執行升版。
        /// </summary>
        public bool Upgraded { get; set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        public GRecipeUpgradeContext()
        {
            OriginalVersion = 1;
            TargetVersion = 1;
            Upgraded = false;
        }
    }
}