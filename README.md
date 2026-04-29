# 📦 GUtility

## 簡介
GUtility 是一套針對 AOI（Automated Optical Inspection）與工業自動化設備整合所設計的 C# 模組化工具庫。

核心目標：
- 降低客製化專案重工
- 建立可重用設備控制模組
- 統一通用功能（Log / Recipe / AutoPurge）
- 提升系統整合效率與維護性

開發基礎：
- .NET Framework `4.7.2`

---

## 功能特色 🚀
- 🧱 模組化架構（Camera / IO / Motor / Modbus）
- 🔌 工業設備整合
- 📝 通用功能模組（Log / Recipe / AutoPurge）
- ⚙️ Modbus Server / Client
- 🔄 易於擴展不同品牌設備
- 🧩 適合作為 AOI 軟體基礎框架

---

## 專案結構
```text
GUtility
├── Common
│   ├── AutoPurge
│   ├── Ini (規劃中)
│   ├── Log
│   └── Recipe
│
├── Modbus
├── Module
│   ├── GCamera
│   └── IO
│
└── MotorController
```

## Common（通用模組）

##AutoPurge

自動清除檔案機制（避免硬碟爆滿）

- 超過容量門檻（如 80%）自動清除
- 依建立時間刪除最舊檔案

## Log

系統記錄功能（.txt）

- 錯誤 / 警告 / 系統資訊
- 支援除錯與追蹤


## Recipe

配方管理模組

- 支援 XML / JSON
- 泛型設計
- 適用 AOI 參數 / ROI


## Ini（規劃中）
- 設備設定
- 路徑 / 初始化參數


## Modbus（通訊模組）

已實作：

- Server
- Client

用途：

- IO / AIO 控制
- 設備通訊
- AOI 結果輸出


## Module（設備模組）
GCamera（相機模組）

目前支援：

✔ Basler
✔ Hikvision（第一版）

規劃中：

- Dalsa

功能：

- Open() / Close()
- StartGrabbing() / StopGrabbing()
- SoftTrigger()
- SetExposure() / SetGain() / SetFrameRate()
- ImageGrabbed 影像事件

設計特點：

- Provider 架構（多品牌擴展）
- SDK 隔離（不依賴廠商 API）
- Thread-safe
- 背景取像（非同步）


## IO（DIO / AIO）

目前支援：

- 聚英 DAM0888
- 聚英 DAM8DA

功能：

- DIO 控制
- AIO 輸出
- Modbus 整合


## MotorController（馬達）

目前支援：

- Oriental Motor（Modbus RS485）

功能：

- Move / Stop
- 狀態讀取


## 設計目標

GUtility 是：

- AOI 模組化工具庫
- 設備整合框架
- 可重用元件集合

讓專案可以：

- 快速組合功能
- 降低重複開發
- 提升維護性


## 開發環境
- Language：C#
- Framework：.NET Framework 4.7.2
- IDE：Visual Studio


## 專案狀態
### 已完成
- Common.Log
- Common.Recipe
- Common.AutoPurge
- Modbus（Server / Client）
- GCamera（Basler / Hikvision）
- IO（DAM0888 / DAM8DA）
- MotorController（RS485）

### 開發中
- Ini 功能
- Dalsa 相機
- Modbus 通用 API
- 更多設備模組
- Example 範例


## License

MIT License
Copyright (c) 2026 Garnett.C