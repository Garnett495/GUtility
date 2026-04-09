using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Recipe.Core;

namespace GUtility.Common.Recipe.Serialization
{
    /// <summary>
    /// Recipe 序列化器介面。
    /// </summary>
    public interface IGRecipeSerializer
    {
        /// <summary>
        /// 對應的 Recipe 格式。
        /// </summary>
        GRecipeFormat Format { get; }

        /// <summary>
        /// 對應的副檔名，不含點號。
        /// 例如：xml、json
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// 將 Recipe 文件序列化為字串。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="document">Recipe 文件。</param>
        /// <returns>序列化後字串。</returns>
        string Serialize<T>(GRecipeDocument<T> document) where T : class, new();

        /// <summary>
        /// 將字串反序列化為 Recipe 文件。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="content">序列化字串內容。</param>
        /// <returns>反序列化後的 Recipe 文件。</returns>
        GRecipeDocument<T> Deserialize<T>(string content) where T : class, new();
    }
}
