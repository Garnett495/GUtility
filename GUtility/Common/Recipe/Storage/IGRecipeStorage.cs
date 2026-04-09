using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Recipe.Storage
{
    /// <summary>
    /// Recipe 儲存層介面。
    /// </summary>
    public interface IGRecipeStorage
    {
        /// <summary>
        /// 確保指定資料夾存在，不存在時自動建立。
        /// </summary>
        /// <param name="folderPath">資料夾路徑。</param>
        void EnsureDirectory(string folderPath);

        /// <summary>
        /// 將文字內容寫入指定檔案。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <param name="content">要寫入的內容。</param>
        void WriteAllText(string filePath, string content);

        /// <summary>
        /// 讀取指定檔案的全部文字內容。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>檔案內容。</returns>
        string ReadAllText(string filePath);

        /// <summary>
        /// 檢查檔案是否存在。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>存在回傳 true，否則 false。</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// 刪除指定檔案。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>成功刪除或檔案原本不存在時回傳 true；失敗回傳 false。</returns>
        bool DeleteFile(string filePath);

        /// <summary>
        /// 取得指定資料夾下符合搜尋條件的檔案路徑清單。
        /// </summary>
        /// <param name="folderPath">資料夾路徑。</param>
        /// <param name="searchPattern">搜尋條件，例如 *.xml、*.json。</param>
        /// <returns>檔案路徑清單。</returns>
        string[] GetFiles(string folderPath, string searchPattern);

        /// <summary>
        /// 複製檔案。
        /// </summary>
        /// <param name="sourceFilePath">來源完整檔案路徑。</param>
        /// <param name="targetFilePath">目標完整檔案路徑。</param>
        /// <param name="overwrite">若目標檔案存在，是否覆蓋。</param>
        void CopyFile(string sourceFilePath, string targetFilePath, bool overwrite);

        /// <summary>
        /// 取得檔名（不含副檔名）。
        /// </summary>
        /// <param name="filePath">完整檔案路徑。</param>
        /// <returns>檔名。</returns>
        string GetFileNameWithoutExtension(string filePath);

        /// <summary>
        /// 組合路徑。
        /// </summary>
        /// <param name="paths">路徑片段。</param>
        /// <returns>組合後路徑。</returns>
        string CombinePath(params string[] paths);
    }
}