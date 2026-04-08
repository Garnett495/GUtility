using System;
using System.Collections.Generic;

namespace GUtility.Log
{
    /// <summary>
    /// 提供外部呼叫的 Log 入口。
    /// 使用方式：
    /// GLog.Init(...)
    /// GLog.Info(...)
    /// GLog.Error(...)
    /// </summary>
    public static class GLog
    {
        private static readonly object _initLock = new object();
        private static GLogManager _manager;

        /// <summary>
        /// 是否已初始化。
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return _manager != null;
            }
        }

        /// <summary>
        /// 初始化 Log 系統。
        /// 正常情況下建議在程式啟動時呼叫一次。
        /// </summary>
        public static void Init(GLogConfig config)
        {
            lock (_initLock)
            {
                if (_manager != null)
                {
                    _manager.Dispose();
                    _manager = null;
                }

                _manager = new GLogManager(config);
            }
        }

        /// <summary>
        /// 釋放 Log 系統。
        /// 建議在程式結束前呼叫。
        /// </summary>
        public static void Shutdown()
        {
            lock (_initLock)
            {
                if (_manager != null)
                {
                    _manager.Dispose();
                    _manager = null;
                }
            }
        }

        /// <summary>
        /// 寫入 Debug。
        /// </summary>
        public static void Debug(string source, string message)
        {
            WriteInternal(GLogLevel.Debug, source, message, null, null);
        }

        /// <summary>
        /// 寫入 Info。
        /// </summary>
        public static void Info(string source, string message)
        {
            WriteInternal(GLogLevel.Info, source, message, null, null);
        }

        /// <summary>
        /// 寫入 Warn。
        /// </summary>
        public static void Warn(string source, string message)
        {
            WriteInternal(GLogLevel.Warn, source, message, null, null);
        }

        /// <summary>
        /// 寫入 Error。
        /// </summary>
        public static void Error(string source, string message, Exception ex)
        {
            WriteInternal(GLogLevel.Error, source, message, ex, null);
        }

        /// <summary>
        /// 寫入 Fatal。
        /// </summary>
        public static void Fatal(string source, string message, Exception ex)
        {
            WriteInternal(GLogLevel.Fatal, source, message, ex, null);
        }

        /// <summary>
        /// 寫入帶有 Properties 的 Info。
        /// </summary>
        public static void Info(string source, string message, Dictionary<string, string> properties)
        {
            WriteInternal(GLogLevel.Info, source, message, null, properties);
        }

        /// <summary>
        /// 寫入帶有 Properties 的 Warn。
        /// </summary>
        public static void Warn(string source, string message, Dictionary<string, string> properties)
        {
            WriteInternal(GLogLevel.Warn, source, message, null, properties);
        }

        /// <summary>
        /// 寫入帶有 Properties 的 Error。
        /// </summary>
        public static void Error(string source, string message, Exception ex, Dictionary<string, string> properties)
        {
            WriteInternal(GLogLevel.Error, source, message, ex, properties);
        }

        /// <summary>
        /// 內部共用寫入方法。
        /// </summary>
        private static void WriteInternal(
            GLogLevel level,
            string source,
            string message,
            Exception ex,
            Dictionary<string, string> properties)
        {
            if (_manager == null)
                return;

            GLogEntry entry = new GLogEntry();
            entry.Level = level;
            entry.Source = source;
            entry.Message = message;
            entry.Exception = ex;

            if (properties != null)
            {
                foreach (KeyValuePair<string, string> pair in properties)
                {
                    entry.Properties[pair.Key] = pair.Value;
                }
            }

            _manager.Enqueue(entry);
        }

        /// <summary>
        /// 手動 Flush。
        /// </summary>
        public static void Flush()
        {
            if (_manager != null)
            {
                _manager.Flush();
            }
        }
    }
}