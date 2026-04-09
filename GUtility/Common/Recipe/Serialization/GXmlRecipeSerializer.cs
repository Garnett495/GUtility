using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using GUtility.Common.Recipe.Core;


namespace GUtility.Common.Recipe.Serialization
{
    /// <summary>
    /// XML Recipe 序列化器。
    /// </summary>
    public class GXmlRecipeSerializer : IGRecipeSerializer
    {
        /// <summary>
        /// 對應格式。
        /// </summary>
        public GRecipeFormat Format
        {
            get { return GRecipeFormat.Xml; }
        }

        /// <summary>
        /// 對應副檔名。
        /// </summary>
        public string FileExtension
        {
            get { return "xml"; }
        }

        /// <summary>
        /// 將 Recipe 文件序列化為 XML 字串。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="document">Recipe 文件。</param>
        /// <returns>XML 字串。</returns>
        public string Serialize<T>(GRecipeDocument<T> document) where T : class, new()
        {
            if (document == null)
                throw new ArgumentNullException("document");

            XmlSerializer serializer = new XmlSerializer(typeof(GRecipeDocument<T>));

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = true;
            settings.OmitXmlDeclaration = false;

            using (StringWriter stringWriter = new Utf8StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.Serialize(xmlWriter, document);
                }

                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// 將 XML 字串反序列化為 Recipe 文件。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="content">XML 字串。</param>
        /// <returns>Recipe 文件。</returns>
        public GRecipeDocument<T> Deserialize<T>(string content) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException("content");

            XmlSerializer serializer = new XmlSerializer(typeof(GRecipeDocument<T>));

            using (StringReader stringReader = new StringReader(content))
            {
                object result = serializer.Deserialize(stringReader);
                GRecipeDocument<T> document = result as GRecipeDocument<T>;

                if (document == null)
                    throw new InvalidOperationException("Failed to deserialize XML content to GRecipeDocument.");

                if (document.Data == null)
                    document.Data = new T();

                return document;
            }
        }

        /// <summary>
        /// 讓 XML 輸出使用 UTF-8。
        /// </summary>
        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }
}
