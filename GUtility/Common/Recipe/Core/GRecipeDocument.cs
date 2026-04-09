using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Recipe.Core
{
    /// <summary>
    /// 通用 Recipe 文件物件。
    /// 用來包裝實際資料與必要的中繼資訊。
    /// </summary>
    /// <typeparam name="T">Recipe 內部資料型別。</typeparam>
    [Serializable]
    public class GRecipeDocument<T> where T : class, new()
    {
        /// <summary>
        /// Recipe 名稱。
        /// </summary>
        public string RecipeName { get; set; }

        /// <summary>
        /// Recipe 儲存格式。
        /// </summary>
        public GRecipeFormat Format { get; set; }

        /// <summary>
        /// 資料結構版本。
        /// 用於未來 class 結構異動時做升版處理。
        /// </summary>
        public int SchemaVersion { get; set; }

        /// <summary>
        /// 建立時間。
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最後修改時間。
        /// </summary>
        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 實際要儲存的資料內容。
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// 建構子。
        /// </summary>
        public GRecipeDocument()
        {
            RecipeName = string.Empty;
            Format = GRecipeFormat.Json;
            SchemaVersion = 1;
            CreatedTime = DateTime.Now;
            ModifiedTime = DateTime.Now;
            Data = new T();
        }

        /// <summary>
        /// 建立新的 Recipe 文件。
        /// </summary>
        /// <param name="recipeName">Recipe 名稱。</param>
        /// <param name="format">儲存格式。</param>
        /// <returns>新的 Recipe 文件物件。</returns>
        public static GRecipeDocument<T> Create(string recipeName, GRecipeFormat format)
        {
            GRecipeDocument<T> document = new GRecipeDocument<T>();
            document.RecipeName = recipeName ?? string.Empty;
            document.Format = format;
            document.SchemaVersion = 1;
            document.CreatedTime = DateTime.Now;
            document.ModifiedTime = DateTime.Now;
            document.Data = new T();
            return document;
        }

        /// <summary>
        /// 更新最後修改時間。
        /// </summary>
        public void Touch()
        {
            ModifiedTime = DateTime.Now;
        }
    }
}