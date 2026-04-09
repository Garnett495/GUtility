using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using GUtility.Common.Recipe.Core;


namespace GUtility.Common.Recipe.Serialization
{
    /// <summary>
    /// JSON Recipe 序列化器。
    /// </summary>
    public class GJsonRecipeSerializer : IGRecipeSerializer
    {
        /// <summary>
        /// 對應格式。
        /// </summary>
        public GRecipeFormat Format
        {
            get { return GRecipeFormat.Json; }
        }

        /// <summary>
        /// 對應副檔名。
        /// </summary>
        public string FileExtension
        {
            get { return "json"; }
        }

        /// <summary>
        /// 將 Recipe 文件序列化為 JSON 字串。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="document">Recipe 文件。</param>
        /// <returns>JSON 字串。</returns>
        public string Serialize<T>(GRecipeDocument<T> document) where T : class, new()
        {
            if (document == null)
                throw new ArgumentNullException("document");

            DataContractJsonSerializer serializer =
                new DataContractJsonSerializer(typeof(GRecipeDocument<T>));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, document);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// 將 JSON 字串反序列化為 Recipe 文件。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="content">JSON 字串。</param>
        /// <returns>Recipe 文件。</returns>
        public GRecipeDocument<T> Deserialize<T>(string content) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException("content");

            DataContractJsonSerializer serializer =
                new DataContractJsonSerializer(typeof(GRecipeDocument<T>));

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                object result = serializer.ReadObject(ms);
                GRecipeDocument<T> document = result as GRecipeDocument<T>;

                if (document == null)
                    throw new InvalidOperationException("Failed to deserialize JSON content to GRecipeDocument.");

                if (document.Data == null)
                    document.Data = new T();

                return document;
            }
        }
    }
}
