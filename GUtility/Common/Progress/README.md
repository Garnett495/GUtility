## 📦 GUtility.Common.Progress

## 簡介

提供一個基於 **SunnyUI** 的通用進度視窗，用於設備初始化、參數讀取、連線流程等需要顯示進度的場景。

## 功能特色 🚀

* 🎯 簡單易用的進度顯示
* 🔄 支援動態更新百分比與訊息
* 🧵 Thread-safe（可跨執行緒更新 UI）
* 🧩 可重複使用於不同專案
* 🎨 與 SunnyUI 風格一致

## 架構設計

```text
GProgressService
    ↓
GProgressForm (UI)
    ↓
UIProcessBar + UILabel
```

## 核心類別

### `GProgressService`

負責控制進度視窗的顯示、更新與關閉。

### `GProgressForm`

進度 UI 視窗（使用 SunnyUI）。

### `GProgressInfo`

進度資料模型（Title / Percent / Message）。

## 使用方式

### 基本用法

```csharp
using GUtility.Common.Progress;

using (GProgressService progress = new GProgressService())
{
    progress.Show("System Initializing", "Preparing...");

    progress.Report(10, "Loading config...");
    progress.Report(30, "Connecting camera...");
    progress.Report(60, "Connecting IO...");
    progress.Report(90, "Initializing system...");

    progress.Finish("Completed");
}
```

### 使用 `GProgressInfo`

```csharp
progress.Show(new GProgressInfo("Recipe Loading", 0, "Start..."));

progress.Report(new GProgressInfo("Recipe Loading", 50, "Reading file..."));

progress.Finish("Done");
```

## UI 元件需求

Designer 中需包含：

```text
lbl_Message
pb_Progress
```

## 注意事項

* `UIProcessBar` 在 SunnyUI 3.9.6.0 不支援 `Minimum`，僅設定：

```csharp
pb_Progress.Maximum = 100;
```

* 建議初始化流程使用，不建議長時間常駐。
* 建置前避免開啟 Designer，避免 COM 錯誤。

## 適用場景

* 設備初始化（Camera / IO / PLC）
* Recipe 載入
* 系統啟動流程
* 批次資料處理
