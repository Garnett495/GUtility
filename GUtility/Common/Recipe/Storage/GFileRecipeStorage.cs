using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace GUtility.Common.Recipe.Storage
{
    /// <summary>
    /// 以本機檔案系統為基礎的 Recipe 儲存層。
    /// </summary>
    public class GFileRecipeStorage : IGRecipeStorage
    {
        /// <summary>
        /// 確保指定資料夾存在，不存在時自動建立。
        /// </summary>
        /// <param name="folderPath">資料夾路徑。</param>
        public void EnsureDirectory(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentNullException("folderPath");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }

        /// <summary>
        /// 將文字內容寫入指定檔案。
        /// 若上層資料夾不存在，會自動建立。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <param name="content">要寫入的內容。</param>
        public void WriteAllText(string filePath, string content)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath");

            if (content == null)
                content = string.Empty;

            string directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                EnsureDirectory(directoryPath);
            }

            File.WriteAllText(filePath, content, Encoding.UTF8);
        }

        /// <summary>
        /// 讀取指定檔案的全部文字內容。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>檔案內容。</returns>
        public string ReadAllText(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Recipe file not found.", filePath);

            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        /// <summary>
        /// 檢查檔案是否存在。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>存在回傳 true，否則 false。</returns>
        public bool FileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            return File.Exists(filePath);
        }

        /// <summary>
        /// 刪除指定檔案。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>成功刪除或檔案原本不存在時回傳 true；失敗回傳 false。</returns>
        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            if (!File.Exists(filePath))
                return true;

            try
            {
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 取得指定資料夾下符合搜尋條件的檔案路徑清單。
        /// </summary>
        /// <param name="folderPath">資料夾路徑。</param>
        /// <param name="searchPattern">搜尋條件，例如 *.xml、*.json。</param>
        /// <returns>檔案路徑清單。</returns>
        public string[] GetFiles(string folderPath, string searchPattern)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentNullException("folderPath");

            if (string.IsNullOrWhiteSpace(searchPattern))
                throw new ArgumentNullException("searchPattern");

            if (!Directory.Exists(folderPath))
                return new string[0];

            return Directory.GetFiles(folderPath, searchPattern, SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// 複製檔案。
        /// </summary>
        /// <param name="sourceFilePath">來源完整檔案路徑。</param>
        /// <param name="targetFilePath">目標完整檔案路徑。</param>
        /// <param name="overwrite">若目標檔案存在，是否覆蓋。</param>
        public void CopyFile(string sourceFilePath, string targetFilePath, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                throw new ArgumentNullException("sourceFilePath");

            if (string.IsNullOrWhiteSpace(targetFilePath))
                throw new ArgumentNullException("targetFilePath");

            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Source file not found.", sourceFilePath);

            string directoryPath = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                EnsureDirectory(directoryPath);
            }

            File.Copy(sourceFilePath, targetFilePath, overwrite);
        }

        /// <summary>
        /// 取得檔名（不含副檔名）。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>檔名。</returns>
        public string GetFileNameWithoutExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// 組合路徑。
        /// </summary>
        /// <param name="paths">路徑片段。</param>
        /// <returns>組合後路徑。</returns>
        public string CombinePath(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            return Path.Combine(paths);
        }
    }
}
