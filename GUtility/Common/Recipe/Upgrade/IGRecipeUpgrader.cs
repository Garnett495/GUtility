using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Recipe.Core;

namespace GUtility.Common.Recipe.Upgrade
{
    /// <summary>
    /// Recipe 升版器介面。
    /// </summary>
    /// <typeparam name="T">Recipe 資料型別。</typeparam>
    public interface IGRecipeUpgrader<T> where T : class, new()
    {
        /// <summary>
        /// 支援的來源版本。
        /// </summary>
        int SourceVersion { get; }

        /// <summary>
        /// 升級後的目標版本。
        /// </summary>
        int TargetVersion { get; }

        /// <summary>
        /// 將指定 Recipe 文件由來源版本升級至目標版本。
        /// </summary>
        /// <param name="document">待升級的 Recipe 文件。</param>
        /// <returns>升級後的 Recipe 文件。</returns>
        GRecipeDocument<T> Upgrade(GRecipeDocument<T> document);
    }
}
