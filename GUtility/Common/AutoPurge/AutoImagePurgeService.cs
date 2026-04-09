using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace GUtility.Common.Storage
{
    /// <summary>
    /// 影像檔案自動清理服務。
    /// 當磁碟使用率超過門檻時，會從指定路徑內最舊的檔案開始刪除，
    /// 直到磁碟使用率回到安全值以下。
    /// </summary>
    public class AutoImagePurgeService : IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly AutoImagePurgeOptions _options;

        private Timer _timer;
        private bool _isPurging;
        private bool _disposed;

        /// <summary>
        /// 是否啟動中。
        /// </summary>
        public bool IsRunning { get; private set; }

        public AutoImagePurgeService(AutoImagePurgeOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (string.IsNullOrWhiteSpace(options.TargetFolder))
                throw new ArgumentException("TargetFolder 不可為空。", "options");

            _options = options;
        }

        /// <summary>
        /// 啟動自動清理監控。
        /// </summary>
        public void Start()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);

            lock (_syncRoot)
            {
                if (IsRunning)
                    return;

                int period = Math.Max(1, _options.CheckIntervalSeconds) * 1000;

                _timer = new Timer(OnTimerCallback, null, period, period);
                IsRunning = true;
            }
        }

        /// <summary>
        /// 停止自動清理監控。
        /// </summary>
        public void Stop()
        {
            lock (_syncRoot)
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }

                IsRunning = false;
            }
        }

        /// <summary>
        /// 立即手動執行一次檢查與清理。
        /// </summary>
        public AutoImagePurgeResult ExecuteNow()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
            return PurgeInternal();
        }

        private void OnTimerCallback(object state)
        {
            try
            {
                PurgeInternal();
            }
            catch
            {
                // 不往外拋，避免背景清理影響主流程
            }
        }

        private AutoImagePurgeResult PurgeInternal()
        {
            lock (_syncRoot)
            {
                if (_isPurging)
                {
                    return new AutoImagePurgeResult
                    {
                        IsSuccess = false,
                        IsPurged = false,
                        Message = "Purge 正在執行中，本次略過。"
                    };
                }

                _isPurging = true;
            }

            try
            {
                if (!Directory.Exists(_options.TargetFolder))
                {
                    return new AutoImagePurgeResult
                    {
                        IsSuccess = false,
                        IsPurged = false,
                        Message = "目標資料夾不存在。"
                    };
                }

                ValidateOptions();

                DriveInfo drive = GetDriveInfo(_options.TargetFolder);
                double usedPercent = GetDriveUsedPercent(drive);

                if (usedPercent < _options.StartPurgeUsedPercent)
                {
                    return new AutoImagePurgeResult
                    {
                        IsSuccess = true,
                        IsPurged = false,
                        Message = "目前磁碟容量尚未達到清理門檻。"
                    };
                }

                AutoImagePurgeResult result = new AutoImagePurgeResult();
                DateTime safeDeleteBefore = DateTime.Now.AddSeconds(-_options.MinimumFileAgeSeconds);

                List<FileInfo> candidateFiles = GetCandidateFiles(safeDeleteBefore);

                foreach (FileInfo file in candidateFiles)
                {
                    usedPercent = GetDriveUsedPercent(drive);

                    if (usedPercent <= _options.StopPurgeUsedPercent)
                    {
                        result.IsSuccess = true;
                        result.IsPurged = true;
                        result.Message = "已清理至安全容量。";
                        return result;
                    }

                    long fileSize = 0;

                    try
                    {
                        if (!file.Exists)
                            continue;

                        fileSize = file.Length;

                        if (file.IsReadOnly)
                            file.IsReadOnly = false;

                        file.Delete();

                        result.DeletedFileCount++;
                        result.DeletedBytes += fileSize;
                        result.DeletedFiles.Add(file.FullName);
                    }
                    catch
                    {
                        // 可能是檔案被占用、剛好被其他執行緒操作、權限問題
                        // 直接略過，避免整體 purge 中斷
                    }
                }

                usedPercent = GetDriveUsedPercent(drive);

                result.IsSuccess = true;
                result.IsPurged = result.DeletedFileCount > 0;
                result.Message = usedPercent <= _options.StopPurgeUsedPercent
                    ? "已清理至安全容量。"
                    : "已刪除可刪檔案，但容量仍未降到停止門檻。";

                return result;
            }
            catch (Exception ex)
            {
                return new AutoImagePurgeResult
                {
                    IsSuccess = false,
                    IsPurged = false,
                    Message = "Purge 發生錯誤: " + ex.Message
                };
            }
            finally
            {
                lock (_syncRoot)
                {
                    _isPurging = false;
                }
            }
        }

        /// <summary>
        /// 取得候選刪除檔案，依建立時間由舊到新排序。
        /// </summary>
        private List<FileInfo> GetCandidateFiles(DateTime safeDeleteBefore)
        {
            List<FileInfo> result = new List<FileInfo>();
            SearchOption searchOption = _options.IncludeSubdirectories
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            foreach (string pattern in _options.SearchPatterns)
            {
                string[] files = Directory.GetFiles(_options.TargetFolder, pattern, searchOption);

                foreach (string filePath in files)
                {
                    try
                    {
                        FileInfo file = new FileInfo(filePath);

                        if (!file.Exists)
                            continue;

                        // 避免刪到剛產生、可能還在寫入中的檔案
                        if (file.CreationTime > safeDeleteBefore)
                            continue;

                        result.Add(file);
                    }
                    catch
                    {
                    }
                }
            }

            return result
                .OrderBy(f => f.CreationTime)
                .ThenBy(f => f.FullName)
                .ToList();
        }

        /// <summary>
        /// 取得指定路徑所在的磁碟資訊。
        /// </summary>
        private DriveInfo GetDriveInfo(string folderPath)
        {
            string root = Path.GetPathRoot(Path.GetFullPath(folderPath));
            return new DriveInfo(root);
        }

        /// <summary>
        /// 取得磁碟使用率。
        /// </summary>
        private double GetDriveUsedPercent(DriveInfo drive)
        {
            long total = drive.TotalSize;
            long free = drive.AvailableFreeSpace;

            if (total <= 0)
                return 0;

            long used = total - free;
            return (double)used * 100.0 / (double)total;
        }

        private void ValidateOptions()
        {
            if (_options.StartPurgeUsedPercent <= 0 || _options.StartPurgeUsedPercent > 100)
                throw new InvalidOperationException("StartPurgeUsedPercent 必須介於 1 ~ 100。");

            if (_options.StopPurgeUsedPercent <= 0 || _options.StopPurgeUsedPercent > 100)
                throw new InvalidOperationException("StopPurgeUsedPercent 必須介於 1 ~ 100。");

            if (_options.StopPurgeUsedPercent >= _options.StartPurgeUsedPercent)
                throw new InvalidOperationException("StopPurgeUsedPercent 必須小於 StartPurgeUsedPercent。");

            if (_options.SearchPatterns == null || _options.SearchPatterns.Count == 0)
                throw new InvalidOperationException("SearchPatterns 不可為空。");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _disposed = true;
        }
    }
}
