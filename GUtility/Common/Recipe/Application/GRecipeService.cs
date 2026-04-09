using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using GUtility.Common.Recipe.Comparison;
using GUtility.Common.Recipe.Core;
using GUtility.Common.Recipe.Serialization;
using GUtility.Common.Recipe.Storage;

namespace GUtility.Common.Recipe.Application
{
    /// <summary>
    /// Recipe 功能主入口。
    /// </summary>
    public class GRecipeService
    {
        private readonly string _rootFolder;
        private readonly IGRecipeStorage _storage;
        private readonly IGRecipeSerializer _xmlSerializer;
        private readonly IGRecipeSerializer _jsonSerializer;
        private readonly GRecipeComparer _comparer;

        /// <summary>
        /// 建立 Recipe 服務。
        /// </summary>
        /// <param name="rootFolder">Recipe 根目錄。</param>
        public GRecipeService(string rootFolder)
            : this(
                  rootFolder,
                  new GFileRecipeStorage(),
                  new GXmlRecipeSerializer(),
                  new GJsonRecipeSerializer(),
                  new GRecipeComparer())
        {
        }

        /// <summary>
        /// 建立 Recipe 服務。
        /// </summary>
        /// <param name="rootFolder">Recipe 根目錄。</param>
        /// <param name="storage">儲存層。</param>
        /// <param name="xmlSerializer">XML 序列化器。</param>
        /// <param name="jsonSerializer">JSON 序列化器。</param>
        /// <param name="comparer">比對器。</param>
        public GRecipeService(
            string rootFolder,
            IGRecipeStorage storage,
            IGRecipeSerializer xmlSerializer,
            IGRecipeSerializer jsonSerializer,
            GRecipeComparer comparer)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
                throw new ArgumentNullException("rootFolder");

            if (storage == null)
                throw new ArgumentNullException("storage");

            if (xmlSerializer == null)
                throw new ArgumentNullException("xmlSerializer");

            if (jsonSerializer == null)
                throw new ArgumentNullException("jsonSerializer");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            _rootFolder = rootFolder;
            _storage = storage;
            _xmlSerializer = xmlSerializer;
            _jsonSerializer = jsonSerializer;
            _comparer = comparer;

            _storage.EnsureDirectory(_rootFolder);
        }

        /// <summary>
        /// Recipe 根目錄。
        /// </summary>
        public string RootFolder
        {
            get { return _rootFolder; }
        }

        /// <summary>
        /// 建立新的 Recipe 文件。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="recipeName">Recipe 名稱。</param>
        /// <param name="format">儲存格式。</param>
        /// <returns>新的 Recipe 文件。</returns>
        public GRecipeDocument<T> CreateNew<T>(string recipeName, GRecipeFormat format)
            where T : class, new()
        {
            ValidateRecipeName(recipeName);

            return GRecipeDocument<T>.Create(recipeName, format);
        }

        /// <summary>
        /// 儲存 Recipe。
        /// 使用 document 內的 RecipeName 與 Format 決定儲存位置。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="document">Recipe 文件。</param>
        public void Save<T>(GRecipeDocument<T> document)
            where T : class, new()
        {
            if (document == null)
                throw new ArgumentNullException("document");

            ValidateRecipeName(document.RecipeName);

            document.Touch();

            string filePath = BuildFilePath(document.RecipeName, document.Format);
            string content = GetSerializer(document.Format).Serialize(document);

            _storage.WriteAllText(filePath, content);
        }

        /// <summary>
        /// 另存 Recipe。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="document">Recipe 文件。</param>
        /// <param name="newRecipeName">新 Recipe 名稱。</param>
        /// <param name="format">新儲存格式。</param>
        /// <returns>另存後的 Recipe 文件。</returns>
        public GRecipeDocument<T> SaveAs<T>(
            GRecipeDocument<T> document,
            string newRecipeName,
            GRecipeFormat format)
            where T : class, new()
        {
            if (document == null)
                throw new ArgumentNullException("document");

            ValidateRecipeName(newRecipeName);

            GRecipeDocument<T> newDocument = CopyDocument(document);
            newDocument.RecipeName = newRecipeName;
            newDocument.Format = format;
            newDocument.Touch();

            Save(newDocument);

            return newDocument;
        }

        /// <summary>
        /// 載入 Recipe。
        /// 會先依指定格式尋找檔案。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="recipeName">Recipe 名稱，不含副檔名。</param>
        /// <param name="format">儲存格式。</param>
        /// <returns>Recipe 文件。</returns>
        public GRecipeDocument<T> Load<T>(string recipeName, GRecipeFormat format)
            where T : class, new()
        {
            ValidateRecipeName(recipeName);

            string filePath = BuildFilePath(recipeName, format);

            if (!_storage.FileExists(filePath))
                throw new FileNotFoundException("Recipe file not found.", filePath);

            string content = _storage.ReadAllText(filePath);
            GRecipeDocument<T> document = GetSerializer(format).Deserialize<T>(content);

            NormalizeDocument(document, recipeName, format);

            return document;
        }

        /// <summary>
        /// 載入 Recipe。
        /// 會先找 JSON，若找不到再找 XML。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="recipeName">Recipe 名稱，不含副檔名。</param>
        /// <returns>Recipe 文件。</returns>
        public GRecipeDocument<T> Load<T>(string recipeName)
            where T : class, new()
        {
            ValidateRecipeName(recipeName);

            string jsonPath = BuildFilePath(recipeName, GRecipeFormat.Json);
            if (_storage.FileExists(jsonPath))
            {
                return Load<T>(recipeName, GRecipeFormat.Json);
            }

            string xmlPath = BuildFilePath(recipeName, GRecipeFormat.Xml);
            if (_storage.FileExists(xmlPath))
            {
                return Load<T>(recipeName, GRecipeFormat.Xml);
            }

            throw new FileNotFoundException("Recipe file not found.", recipeName);
        }

        /// <summary>
        /// 刪除指定 Recipe。
        /// </summary>
        /// <param name="recipeName">Recipe 名稱，不含副檔名。</param>
        /// <param name="format">儲存格式。</param>
        /// <returns>成功回傳 true，失敗回傳 false。</returns>
        public bool Delete(string recipeName, GRecipeFormat format)
        {
            ValidateRecipeName(recipeName);

            string filePath = BuildFilePath(recipeName, format);
            return _storage.DeleteFile(filePath);
        }

        /// <summary>
        /// 檢查指定 Recipe 是否存在。
        /// </summary>
        /// <param name="recipeName">Recipe 名稱，不含副檔名。</param>
        /// <param name="format">儲存格式。</param>
        /// <returns>存在回傳 true，否則 false。</returns>
        public bool Exists(string recipeName, GRecipeFormat format)
        {
            if (string.IsNullOrWhiteSpace(recipeName))
                return false;

            string filePath = BuildFilePath(recipeName, format);
            return _storage.FileExists(filePath);
        }

        /// <summary>
        /// 取得指定格式的 Recipe 檔名清單。
        /// </summary>
        /// <param name="format">儲存格式。</param>
        /// <returns>Recipe 名稱清單（不含副檔名）。</returns>
        public List<string> GetRecipeFiles(GRecipeFormat format)
        {
            string pattern = "*." + GetSerializer(format).FileExtension;
            string[] files = _storage.GetFiles(_rootFolder, pattern);

            List<string> result = new List<string>();
            int i;
            for (i = 0; i < files.Length; i++)
            {
                result.Add(_storage.GetFileNameWithoutExtension(files[i]));
            }

            return result;
        }

        /// <summary>
        /// 複製指定 Recipe。
        /// 格式與來源相同。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="sourceRecipeName">來源 Recipe 名稱。</param>
        /// <param name="newRecipeName">新 Recipe 名稱。</param>
        /// <returns>複製後的 Recipe 文件。</returns>
        public GRecipeDocument<T> Copy<T>(string sourceRecipeName, string newRecipeName)
            where T : class, new()
        {
            ValidateRecipeName(sourceRecipeName);
            ValidateRecipeName(newRecipeName);

            GRecipeDocument<T> source = Load<T>(sourceRecipeName);

            GRecipeDocument<T> copied = CopyDocument(source);
            copied.RecipeName = newRecipeName;
            copied.CreatedTime = DateTime.Now;
            copied.ModifiedTime = DateTime.Now;

            Save(copied);

            return copied;
        }

        /// <summary>
        /// 複製指定 Recipe 並轉換格式。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="sourceRecipeName">來源 Recipe 名稱。</param>
        /// <param name="sourceFormat">來源格式。</param>
        /// <param name="newRecipeName">新 Recipe 名稱。</param>
        /// <param name="targetFormat">目標格式。</param>
        /// <returns>複製後的 Recipe 文件。</returns>
        public GRecipeDocument<T> Copy<T>(
            string sourceRecipeName,
            GRecipeFormat sourceFormat,
            string newRecipeName,
            GRecipeFormat targetFormat)
            where T : class, new()
        {
            ValidateRecipeName(sourceRecipeName);
            ValidateRecipeName(newRecipeName);

            GRecipeDocument<T> source = Load<T>(sourceRecipeName, sourceFormat);

            GRecipeDocument<T> copied = CopyDocument(source);
            copied.RecipeName = newRecipeName;
            copied.Format = targetFormat;
            copied.CreatedTime = DateTime.Now;
            copied.ModifiedTime = DateTime.Now;

            Save(copied);

            return copied;
        }

        /// <summary>
        /// 比較兩個 Recipe。
        /// 會自動尋找檔案格式。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="leftRecipeName">左側 Recipe 名稱。</param>
        /// <param name="rightRecipeName">右側 Recipe 名稱。</param>
        /// <returns>差異清單。</returns>
        public List<GRecipeDifference> Compare<T>(string leftRecipeName, string rightRecipeName)
            where T : class, new()
        {
            GRecipeDocument<T> left = Load<T>(leftRecipeName);
            GRecipeDocument<T> right = Load<T>(rightRecipeName);

            return _comparer.Compare(left, right, GCompareOptions.Default());
        }

        /// <summary>
        /// 比較兩個 Recipe。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="leftRecipeName">左側 Recipe 名稱。</param>
        /// <param name="leftFormat">左側格式。</param>
        /// <param name="rightRecipeName">右側 Recipe 名稱。</param>
        /// <param name="rightFormat">右側格式。</param>
        /// <param name="options">比較選項。</param>
        /// <returns>差異清單。</returns>
        public List<GRecipeDifference> Compare<T>(
            string leftRecipeName,
            GRecipeFormat leftFormat,
            string rightRecipeName,
            GRecipeFormat rightFormat,
            GCompareOptions options)
            where T : class, new()
        {
            GRecipeDocument<T> left = Load<T>(leftRecipeName, leftFormat);
            GRecipeDocument<T> right = Load<T>(rightRecipeName, rightFormat);

            return _comparer.Compare(left, right, options);
        }

        /// <summary>
        /// 比較兩個 Recipe 文件物件。
        /// </summary>
        /// <typeparam name="T">Recipe 資料型別。</typeparam>
        /// <param name="left">左側 Recipe。</param>
        /// <param name="right">右側 Recipe。</param>
        /// <param name="options">比較選項。</param>
        /// <returns>差異清單。</returns>
        public List<GRecipeDifference> Compare<T>(
            GRecipeDocument<T> left,
            GRecipeDocument<T> right,
            GCompareOptions options)
            where T : class, new()
        {
            return _comparer.Compare(left, right, options);
        }

        /// <summary>
        /// 取得指定格式的完整檔案路徑。
        /// </summary>
        /// <param name="recipeName">Recipe 名稱，不含副檔名。</param>
        /// <param name="format">儲存格式。</param>
        /// <returns>完整檔案路徑。</returns>
        public string GetFilePath(string recipeName, GRecipeFormat format)
        {
            ValidateRecipeName(recipeName);
            return BuildFilePath(recipeName, format);
        }

        private IGRecipeSerializer GetSerializer(GRecipeFormat format)
        {
            switch (format)
            {
                case GRecipeFormat.Xml:
                    return _xmlSerializer;

                case GRecipeFormat.Json:
                    return _jsonSerializer;

                default:
                    throw new NotSupportedException("Unsupported recipe format.");
            }
        }

        private string BuildFilePath(string recipeName, GRecipeFormat format)
        {
            string extension = GetSerializer(format).FileExtension;
            string fileName = recipeName + "." + extension;
            return _storage.CombinePath(_rootFolder, fileName);
        }

        private void NormalizeDocument<T>(
            GRecipeDocument<T> document,
            string recipeName,
            GRecipeFormat format)
            where T : class, new()
        {
            if (document == null)
                throw new InvalidOperationException("Recipe document is null.");

            if (string.IsNullOrWhiteSpace(document.RecipeName))
                document.RecipeName = recipeName;

            document.Format = format;

            if (document.SchemaVersion <= 0)
                document.SchemaVersion = 1;

            if (document.Data == null)
                document.Data = new T();
        }

        private GRecipeDocument<T> CopyDocument<T>(GRecipeDocument<T> source)
            where T : class, new()
        {
            if (source == null)
                throw new ArgumentNullException("source");

            IGRecipeSerializer serializer = GetSerializer(source.Format);
            string content = serializer.Serialize(source);
            GRecipeDocument<T> copied = serializer.Deserialize<T>(content);

            if (copied.Data == null)
                copied.Data = new T();

            return copied;
        }

        private void ValidateRecipeName(string recipeName)
        {
            if (string.IsNullOrWhiteSpace(recipeName))
                throw new ArgumentNullException("recipeName");

            char[] invalidChars = Path.GetInvalidFileNameChars();
            int i;
            for (i = 0; i < invalidChars.Length; i++)
            {
                if (recipeName.IndexOf(invalidChars[i]) >= 0)
                    throw new ArgumentException("Recipe name contains invalid file name characters.", "recipeName");
            }
        }
    }
}
