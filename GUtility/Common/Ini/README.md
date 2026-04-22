# GUtility.Common.Ini

## 簡介
`GUtility.Common.Ini` 是一個提供 `.ini` 檔讀寫的輕量化設定模組，適合用於 AOI、自動化設備、工業控制軟體等場景。

此模組主要用來存放**機台層級、系統層級、較少變動**的設定資訊，例如：
- PLC 連線資訊
- 相機序號與基本參數
- 圖片與 Log 儲存路徑
- 軟體啟動選項
- 系統固定參數

---

## 功能特色
- ⚙️ 支援標準 `Section / Key / Value` 結構
- 🔒 支援 thread-safe 設計
- 🧩 支援 `string`、`int`、`double`、`bool`、`enum` 型別讀寫
- 📝 支援預設值讀取，避免缺值造成例外
- 📂 支援檔案不存在時自動建立
- 💾 支援記憶體快取與 `Save()` 顯式儲存
- 🏗️ 預留物件序列化擴充架構

---

## 適用場景
- 設備固定設定
- 系統啟動設定
- 相機 / PLC / IO 基本參數
- 路徑設定
- Debug 開關
- 不常異動的機台資訊

---

## 不建議存放的資料
- 高頻更新資料
- 即時狀態資料
- 檢測結果
- 大量 Recipe 參數
- 複雜巢狀物件
- 大量歷史紀錄

這類資料比較適合使用 `Recipe`、`JSON`、資料庫或其他結構化儲存方式。

---

## 專案結構
```text
GUtility.Common.Ini
├── Abstractions
│   ├── IGIniFile.cs
│   └── IGIniSerializer.cs
├── Core
│   ├── GIniFile.cs
│   ├── GIniSection.cs
│   └── GIniValueConverter.cs
├── Models
│   ├── GIniOptions.cs
│   └── GIniKeyInfo.cs
├── Exceptions
│   ├── GIniException.cs
│   ├── GIniFileNotFoundException.cs
│   └── GIniParseException.cs
├── Serialization
│   └── GIniObjectSerializer.cs
└── Example
    ├── BasicIniExample.cs
    ├── EquipmentConfigExample.cs
    └── TypedReadWriteExample.cs
```

## 架構設計

此模組主要分為以下幾層：

Abstractions
- 定義 IGIniFile
- 定義 IGIniSerializer
- 讓上層呼叫不直接依賴具體實作

Core
- GIniFile：負責 .ini 檔案的讀取、寫入、儲存、重載
- GIniSection：表示單一 Section 結構
- GIniValueConverter：處理型別轉換

Models
- GIniOptions：設定 Ini 模組的行為選項
- GIniKeyInfo：表示單一 Key 資訊

Exceptions
- 提供 Ini 專用例外類別，方便錯誤分類與維護

Serialization
- GIniObjectSerializer：提供物件與 Ini 之間的簡易映射能力

Example
- 提供常見使用範例，方便快速上手


## 設計重點
使用記憶體快取管理 Ini 資料
呼叫 WriteXXX() 時先更新記憶體內容
呼叫 Save() 後再統一寫回硬碟
避免頻繁 I/O 導致效能下降
適合用於設備啟動、初始化、系統設定管理


##支援的資料型別

目前支援以下型別：

- string
- int
- double
- bool
- enum

其中 bool 支援多種常見格式，例如：

- true / false
- 1 / 0
- yes / no
- on / off


## 基本使用方式

建立 GIniFile
```csharp
using GUtility.Common.Ini.Core;

GIniFile ini = new GIniFile(@"D:\Config\Machine.ini");
```

寫入資料
```csharp
ini.WriteString("Machine", "MachineName", "AOI-01");
ini.WriteString("PLC", "Ip", "192.168.0.10");
ini.WriteInt("PLC", "Port", 502);
ini.WriteBool("System", "EnableDebugLog", true);
ini.Save();
```

讀取資料
```csharp
string machineName = ini.ReadString("Machine", "MachineName", "Unknown");
string plcIp = ini.ReadString("PLC", "Ip", "127.0.0.1");
int plcPort = ini.ReadInt("PLC", "Port", 502);
bool enableDebug = ini.ReadBool("System", "EnableDebugLog", false);
```