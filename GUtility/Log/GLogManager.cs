using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GUtility.Log.Writers;

namespace GUtility.Log
{
    /// <summary>
    /// Log 管理核心：
    /// 1. 接收外部傳入的 Log
    /// 2. 透過 Queue 緩衝
    /// 3. 使用背景執行緒非同步寫入
    /// 4. 啟動時清理舊 Log
    /// </summary>
    public class GLogManager : IDisposable
    {
        private readonly GLogConfig _config;
        private readonly IGLogWriter _writer;
        private readonly BlockingCollection<GLogEntry> _queue;
        private readonly Thread _workerThread;

        private bool _disposed;

        /// <summary>
        /// 是否已啟動。
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return !_disposed;
            }
        }

        /// <summary>
        /// 建構子。
        /// </summary>
        public GLogManager(GLogConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _config = config;
            _writer = new GFileLogWriter(_config);
            _queue = new BlockingCollection<GLogEntry>(_config.MaxQueueCount);

            CleanupOldLogs();

            _workerThread = new Thread(ProcessQueue);
            _workerThread.IsBackground = true;
            _workerThread.Name = "GLogWorker";
            _workerThread.Start();
        }

        /// <summary>
        /// 將一筆 Log 放入 Queue，交由背景執行緒處理。
        /// </summary>
        public void Enqueue(GLogEntry entry)
        {
            if (_disposed)
                return;

            if (entry == null)
                return;

            if (entry.Level < _config.MinimumLevel)
                return;

            try
            {
                _queue.Add(entry);
            }
            catch
            {
                // 若 Queue 已關閉或其他狀況，不往外拋例外
            }
        }

        /// <summary>
        /// 背景執行緒持續消化 Queue 中的資料。
        /// </summary>
        private void ProcessQueue()
        {
            try
            {
                foreach (GLogEntry entry in _queue.GetConsumingEnumerable())
                {
                    _writer.Write(entry);
                }
            }
            catch
            {
                // 不讓背景執行緒因例外中止
            }
        }

        /// <summary>
        /// 清理超過保留天數的舊資料夾。
        /// 預設資料夾名稱格式：yyyy-MM-dd
        /// </summary>
        private void CleanupOldLogs()
        {
            try
            {
                if (_config.RetentionDays <= 0)
                    return;

                if (string.IsNullOrWhiteSpace(_config.RootFolder))
                    return;

                if (!Directory.Exists(_config.RootFolder))
                    return;

                string[] directories = Directory.GetDirectories(_config.RootFolder);
                DateTime deadline = DateTime.Now.Date.AddDays(-_config.RetentionDays);

                for (int i = 0; i < directories.Length; i++)
                {
                    string folderPath = directories[i];
                    string folderName = Path.GetFileName(folderPath);

                    DateTime folderDate;
                    if (DateTime.TryParse(folderName, out folderDate))
                    {
                        if (folderDate.Date < deadline)
                        {
                            Directory.Delete(folderPath, true);
                        }
                    }
                }
            }
            catch
            {
                // 清除失敗不影響主流程
            }
        }

        /// <summary>
        /// 手動 Flush。
        /// </summary>
        public void Flush()
        {
            if (_disposed)
                return;

            try
            {
                // 等待 queue 暫時清空
                int waitCount = 0;
                while (_queue.Count > 0 && waitCount < 100)
                {
                    Thread.Sleep(20);
                    waitCount++;
                }

                _writer.Flush();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 釋放資源，並盡量把剩餘的 log 寫完。
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _queue.CompleteAdding();
            }
            catch
            {
            }

            try
            {
                if (_workerThread != null && _workerThread.IsAlive)
                {
                    _workerThread.Join(3000);
                }
            }
            catch
            {
            }

            try
            {
                _writer.Flush();
                _writer.Dispose();
            }
            catch
            {
            }

            try
            {
                _queue.Dispose();
            }
            catch
            {
            }
        }
    }
}