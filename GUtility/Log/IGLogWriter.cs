using System;

namespace GUtility.Log
{
    /// <summary>
    /// Log Writer 介面。
    /// 之後若要擴充 Console / UI / DB 輸出，可共用此介面。
    /// </summary>
    public interface IGLogWriter : IDisposable
    {
        /// <summary>
        /// 寫入一筆 Log。
        /// </summary>
        void Write(GLogEntry entry);

        /// <summary>
        /// 強制 Flush。
        /// </summary>
        void Flush();
    }
}