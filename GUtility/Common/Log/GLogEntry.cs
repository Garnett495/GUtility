using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GUtility.Common.Log
{
    /// <summary>
    /// 一筆 Log 資料。
    /// </summary>
    public class GLogEntry
    {
        /// <summary>記錄時間</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Log 等級</summary>
        public GLogLevel Level { get; set; }

        /// <summary>來源模組，例如 Camera / PLC / Motion / UI</summary>
        public string Source { get; set; }

        /// <summary>主要訊息</summary>
        public string Message { get; set; }

        /// <summary>執行緒 ID</summary>
        public string ThreadId { get; set; }

        /// <summary>例外資訊，可為 null</summary>
        public Exception Exception { get; set; }

        /// <summary>額外屬性，例如 RecipeName / CCDNo / LotId</summary>
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// 建立預設的 LogEntry。
        /// </summary>
        public GLogEntry()
        {
            Timestamp = DateTime.Now;
            ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// 將目前的 Log 轉換成一行文字。
        /// </summary>
        public string ToLineString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append(" [");
            sb.Append(GetLevelText(Level));
            sb.Append("] ");

            sb.Append("[");
            sb.Append(string.IsNullOrWhiteSpace(Source) ? "General" : Source);
            sb.Append("] ");

            sb.Append("[T");
            sb.Append(string.IsNullOrWhiteSpace(ThreadId) ? "0" : ThreadId);
            sb.Append("] ");

            sb.Append(Message ?? string.Empty);

            if (Properties != null && Properties.Count > 0)
            {
                sb.Append(" | ");

                bool isFirst = true;
                foreach (KeyValuePair<string, string> pair in Properties)
                {
                    if (!isFirst)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(pair.Key);
                    sb.Append("=");
                    sb.Append(pair.Value);

                    isFirst = false;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 取得完整例外資訊，包含 InnerException。
        /// </summary>
        public string GetExceptionDetail()
        {
            if (Exception == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            Exception current = Exception;
            int level = 0;

            while (current != null)
            {
                sb.AppendLine("---- Exception Level " + level + " ----");
                sb.AppendLine("Type       : " + current.GetType().FullName);
                sb.AppendLine("Message    : " + current.Message);
                sb.AppendLine("StackTrace :");
                sb.AppendLine(current.StackTrace);

                current = current.InnerException;
                level++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 將 Log 等級格式化成固定寬度文字，方便閱讀。
        /// </summary>
        private string GetLevelText(GLogLevel level)
        {
            switch (level)
            {
                case GLogLevel.Debug:
                    return "DEBUG";
                case GLogLevel.Info:
                    return "INFO ";
                case GLogLevel.Warn:
                    return "WARN ";
                case GLogLevel.Error:
                    return "ERROR";
                case GLogLevel.Fatal:
                    return "FATAL";
                default:
                    return "UNKWN";
            }
        }
    }
}