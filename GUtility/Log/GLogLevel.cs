using System;

namespace GUtility.Log
{
    /// <summary>
    /// Log 等級。
    /// 數值越大表示嚴重程度越高。
    /// </summary>
    public enum GLogLevel
    {
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5
    }
}