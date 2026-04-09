# 📦 GUtility.Common.Storage

## 簡介
提供 AOI / 自動化設備使用的磁碟空間管理工具，透過自動清理指定資料夾內的舊檔案，避免因大量影像資料累積導致磁碟容量不足而影響設備運行。

此模組特別適用於：
- AOI 影像檢測（大量拍照）
- 長時間連續運行設備
- 無人工定期清理的系統環境

---

## 功能特色
🧹 自動清除舊影像檔案  
📊 依磁碟使用率觸發清理（非固定時間刪除）  
🎯 只清指定資料夾（安全性高）  
⏳ 採用「開始/停止」雙門檻避免頻繁觸發  
📂 支援子資料夾掃描  
🛡️ 避免刪除剛建立的檔案  
⚙️ 可手動觸發或背景自動執行  

---

## 使用場景

- AOI 每秒拍照 → 磁碟快速爆滿  
- 長時間 unattended 運行設備  
- 需要穩定且可預期的資料清理機制  

---

## 設計概念

### 1. 磁碟容量驅動（Disk-driven）
不是單純刪檔，而是依磁碟使用率判斷：

- 超過 `StartPurgeUsedPercent` → 開始清理  
- 低於 `StopPurgeUsedPercent` → 停止清理  

👉 避免頻繁觸發（Hysteresis 設計）

---

### 2. 最舊優先刪除（FIFO）
- 依 `CreationTime` 排序
- 先刪最舊資料

👉 符合 AOI 現場使用邏輯（舊資料價值最低）

---

### 3. 安全機制
- 不刪指定資料夾外檔案  
- 不刪剛建立的檔案（避免寫入衝突）  
- 檔案被佔用自動略過  

---

### 4. 非阻塞架構
- 使用 `Timer` 背景執行  
- 不影響 AOI 主流程（拍照 / 檢測）  

---

## 架構
AutoImagePurgeService
↓
AutoImagePurgeOptions → 設定
AutoImagePurgeResult → 執行結果


---

## 快速開始

### 1.初始化設定

```csharp
AutoImagePurgeOptions options = new AutoImagePurgeOptions();
options.TargetFolder = @"D:\AOIImages";
options.StartPurgeUsedPercent = 80;
options.StopPurgeUsedPercent = 70;
options.CheckIntervalSeconds = 30;
options.IncludeSubdirectories = true;
options.MinimumFileAgeSeconds = 30;

```

### 2.啟動服務

```csharp
AutoImagePurgeService purgeService = new AutoImagePurgeService(options);
purgeService.Start();
```

### 3.手動執行（測試用）
```csharp
AutoImagePurgeResult result = purgeService.ExecuteNow();

Console.WriteLine(result.IsSuccess);
Console.WriteLine(result.DeletedFileCount);
Console.WriteLine(result.DeletedBytes);
```

### 4.停止服務
```csharp
purgeService.Dispose();
```
---

## 參數說明
`AutoImagePurgeOptions`
參數	說明
`TargetFolder`	要清理的資料夾
`StartPurgeUsedPercent`	開始清理門檻 (%)
`StopPurgeUsedPercent`	停止清理門檻 (%)
`CheckIntervalSeconds`	檢查週期
`IncludeSubdirectories`	是否包含子資料夾
`SearchPatterns`	要清理的檔案類型
`MinimumFileAgeSeconds`	檔案最小存活時間

方法說明
`Start()`	啟動背景自動清理服務
`Stop()`	停止清理服務
`ExecuteNow()`	立即執行一次清理流程（通常用於測試或手動觸發）

## 清理流程
1. 檢查磁碟使用率
2. 若 < Start → 不動作
3. 若 >= Start → 開始清理
4. 取得所有候選檔案
5. 依建立時間排序（最舊優先）
6. 逐一刪除
7. 每刪一個重新檢查容量
8. 若 <= Stop → 停止

## 使用範例（AOI情境）
```csharp
// AOI 啟動時初始化
purgeService.Start();

// 設備運行中自動清理，不需額外處理

// 關閉時
purgeService.Dispose();
```

##注意事項
⚠️ CreationTime 問題
- 若檔案被複製/搬移，時間可能不準
- 可自行改為 LastWriteTime

⚠️ 避免刪到正在寫入的檔案
- 建議 MinimumFileAgeSeconds >= 10~30

⚠️ 清理頻率
- 建議 10~60 秒
- 不要每秒檢查（浪費 IO）

⚠️ 建議限制檔案類型
```csharp
options.SearchPatterns = new List<string>
{
    "*.bmp",
    "*.jpg",
    "*.png"
};
```

## 延伸建議（進階優化）
🔒 白名單資料夾（不刪某些路徑）
📁 先刪整個日期資料夾（效能更好）
📊 加入 log 紀錄清理歷史
🧠 設備 Busy 時暫停 purge
📉 加入最低保留檔案數（避免全刪）


## License

MIT License

Copyright (c) 2026 Garnett.C

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.