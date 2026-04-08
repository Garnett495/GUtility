using System;
using System.IO;
using System.Text;

namespace GUtility.Log.Writers
{
    /// <summary>
    /// 將 Log 寫入 txt 檔案。
    /// </summary>
    public class GFileLogWriter : IGLogWriter
    {
        private readonly GLogConfig _config;
        private readonly object _fileLock = new object();

        private string _currentDateFolder;
        private string _currentFilePath;
        private bool _disposed;

        /// <summary>
        /// 建構子。
        /// </summary>
        public GFileLogWriter(GLogConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _config = config;
        }

        /// <summary>
        /// 寫入一筆 Log 到 txt。
        /// </summary>
        public void Write(GLogEntry entry)
        {
            if (_disposed)
                return;

            if (entry == null)
                return;

            try
            {
                EnsureFilePath(entry.Timestamp);

                lock (_fileLock)
                {
                    using (StreamWriter writer = new StreamWriter(_currentFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(entry.ToLineString());

                        if (entry.Exception != null)
                        {
                            writer.WriteLine(entry.GetExceptionDetail());
                        }
                    }
                }
            }
            catch
            {
                // Log 本身失敗時不要再往外丟例外，
                // 否則可能讓主程式被 log 反拖垮。
            }
        }

        /// <summary>
        /// 依照日期確保當天的資料夾與檔案路徑存在。
        /// </summary>
        private void EnsureFilePath(DateTime logTime)
        {
            string todayFolderName = logTime.ToString("yyyy-MM-dd");

            if (_currentDateFolder == todayFolderName && !string.IsNullOrWhiteSpace(_currentFilePath))
                return;

            string rootFolder = _config.RootFolder;
            if (string.IsNullOrWhiteSpace(rootFolder))
            {
                rootFolder = "Logs";
            }

            string dayFolderPath = Path.Combine(rootFolder, todayFolderName);

            if (!Directory.Exists(dayFolderPath))
            {
                Directory.CreateDirectory(dayFolderPath);
            }

            string fileName = _config.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "System.log.txt";
            }

            _currentDateFolder = todayFolderName;
            _currentFilePath = Path.Combine(dayFolderPath, fileName);
        }

        /// <summary>
        /// 目前此類別使用 using 寫檔，因此每次都已自動 Flush。
        /// 保留此方法是為了介面一致性。
        /// </summary>
        public void Flush()
        {
            // using (StreamWriter) 已自動刷新
        }

        /// <summary>
        /// 釋放資源。
        /// </summary>
        public void Dispose()
        {
            _disposed = true;
        }
    }
}