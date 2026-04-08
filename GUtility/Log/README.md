# GUtility.Log

一套適用於 **AOI / 設備整合系統（C# / .NET Framework 4.7.2）** 的輕量級 Log 模組。
支援非同步寫入、每日分檔、Exception 詳細記錄與擴充屬性（Context），可直接整合至專案使用。

---

# 🚀 Features

* 非同步寫入（避免阻塞主流程）
* Log 等級分級（`Debug` / `Info` / `Warn` / `Error` / `Fatal`）
* 自動輸出 `.txt` 檔
* 每日資料夾分檔
* 自動建立資料夾
* Exception 完整記錄（含 `StackTrace` / `InnerException`）
* 支援自訂屬性（例如 `RecipeName`、`CCDNo`）
* 自動清除過期 Log
* Thread-safe 設計
* 可擴充 Writer（未來可接 UI / DB / Server）

---

# 📂 Project Structure

```
GUtility
├─ Log
│  ├─ GLog.cs
│  ├─ GLogConfig.cs
│  ├─ GLogEntry.cs
│  ├─ GLogLevel.cs
│  ├─ GLogManager.cs
│  ├─ IGLogWriter.cs
│  └─ Writers
│     └─ GFileLogWriter.cs
└─ Examples
   ├─ Example_BasicUsage.cs
   ├─ Example_ExceptionUsage.cs
   └─ Example_PropertiesUsage.cs
```

---

# ⚙️ Initialization

在程式啟動時初始化 Log：

```csharp
GLogConfig config = new GLogConfig
{
    RootFolder = @"D:\Logs",          // Log 輸出路徑
    MinimumLevel = GLogLevel.Debug,   // 最低輸出等級
    RetentionDays = 30,              // 保留天數
    FileName = "System.log.txt"      // 檔名
};

GLog.Init(config);
```

---

# ✍️ Usage

## 基本使用

```csharp
GLog.Info("System", "System started.");
GLog.Debug("Camera", "Camera initializing...");
GLog.Warn("PLC", "PLC response slow.");
GLog.Error("Inspection", "Inspection failed.");
```

---

## 記錄 Exception

```csharp
try
{
    int a = 0;
    int b = 100 / a;
}
catch (Exception ex)
{
    GLog.Error("Math", "Divide by zero error.", ex);
}
```

---

## 使用 Properties（推薦）

```csharp
var props = new Dictionary<string, string>
{
    { "RecipeName", "Glass_A01" },
    { "CCDNo", "2" },
    { "LotId", "LOT20260408" },
    { "Result", "NG" }
};

GLog.Info("Inspection", "Inspection completed.", props);
```

---

# 📁 Output Example

Log 會輸出至：

```
D:\Logs\2026-04-08\System.log.txt
```

內容範例：

```
2026-04-08 16:32:15.128 [INFO ] [System] [T1] System started.
2026-04-08 16:32:16.442 [DEBUG] [Camera] [T8] Camera initializing...
2026-04-08 16:32:18.903 [WARN ] [PLC] [T10] PLC response slow.
2026-04-08 16:32:20.114 [INFO ] [Inspection] [T15] Inspection completed. | RecipeName=Glass_A01, CCDNo=2
2026-04-08 16:32:25.221 [ERROR] [Math] [T1] Divide by zero error.
---- Exception Level 0 ----
Type       : System.DivideByZeroException
Message    : Attempted to divide by zero.
StackTrace :
...
```

---

# 🔚 Shutdown

建議在程式結束前呼叫：

```csharp
GLog.Flush();
GLog.Shutdown();
```

避免 Log 遺失。

---

# ⚠️ Important Notes

## 1. 路徑權限

確保 `RootFolder` 具有寫入權限，例如：

```csharp
@"D:\Logs"
```

---

## 2. 字串格式

請使用：

```csharp
@"D:\Logs"
```

避免：

```csharp
"D:\Logs" // ❌
```

---

## 3. 高頻 Log 控制

正式環境建議：

```csharp
config.MinimumLevel = GLogLevel.Info;
```

避免產生大量 `Debug` Log。

---

# 🧠 Best Practices（AOI / 設備系統）

建議 Log 記錄以下內容：

* 相機連線 / 斷線
* PLC 通訊
* Recipe 載入
* 檢測結果（OK / NG）
* Timeout / Retry
* 馬達動作
* 異常流程

---

# 🔧 Future Extensions

* 模組分檔（Camera / PLC / Inspection）
* Error Log 分流
* UI 即時 Log 顯示
* JSON 格式 Log
* Database / Remote Log Server

---

# 📌 Summary

這個 Log 模組的設計重點：

* **穩定**
* **不影響主流程**
* **易於擴充**
* **適合設備整合與 AOI 系統**

---

# 🧩 Example

請參考：

```
GUtility/Examples/
```

快速上手。

---

# 🏁 License

Internal Use / Custom Project Use
