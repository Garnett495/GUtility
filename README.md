# 📦 GUtility

## 簡介
GUtility 是一套針對 AOI（Automated Optical Inspection）與工業自動化設備整合所設計的 C# 模組化工具庫。

此專案的核心目標：
- 降低客製化專案重工
- 建立可重用設備控制模組
- 統一常用功能（Log / Recipe / AutoPurge）
- 提升系統整合效率與維護性

目前專案以 `.NET Framework 4.7.2` 為基礎，並持續擴展各類設備控制能力。

---

## 功能特色 🚀
- 🧱 模組化設計（Camera / IO / Motor / Modbus）
- 🔌 支援工業設備整合
- 📝 提供通用功能（Log / Recipe / AutoPurge）
- ⚙️ 支援 Modbus Server / Client
- 🔄 易於擴展不同設備與品牌
- 🧩 適合作為 AOI 軟體基礎框架

---

## 專案結構

```text
GUtility
│
├── Common
│   ├── AutoPurge
│   ├── Ini (未實作)
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

## 各模組功能說明

## Common（通用功能）

此資料夾提供跨專案共用的基礎功能，不依賴特定設備。

### AutoPurge

自動清除檔案機制，避免設備長時間運行造成硬碟容量不足。

功能
- 監控磁碟容量使用率
- 超過門檻（如 80%）開始清除
- 依建立時間刪除最舊檔案
- 清除至安全容量後停止

應用
- AOI 圖片資料夾
- 長時間 Log 累積

### Log

系統記錄功能，支援 .txt 輸出。

功能
- 記錄系統運行資訊
- 記錄錯誤 / 警告
- 支援除錯與問題追蹤

應用
- 設備狀態紀錄
- 通訊異常
- 檢測流程紀錄


### Recipe

參數與配方管理模組。

功能
- 支援 XML / JSON 序列化
- 泛型設計（可擴展不同模型）
- 搭配 enum 避免錯誤輸入

應用
- AOI 檢測參數
- ROI 設定
- 多產品切換


### Ini（尚未實作）

預計提供 .ini 設定檔讀寫功能。

預計用途
- 設備 IP / Port
- 系統初始化設定
- 路徑設定


### Modbus（通訊模組）

提供 Modbus 通訊能力，目前已具備基本架構。

已實作
- Modbus Server
- Modbus Client

功能
- 建立 Server / Client 通訊
- 控制啟動與停止
- 狀態管理（如 IsRunning）
- 未來規劃
- 封裝常用讀寫方法（Coil / Register）
- 建立標準通訊 API
- 提供完整使用範例

應用
- IO / AIO 控制
- 設備間資料交換
- AOI 結果輸出


## Module（設備模組）

此層負責「實際設備控制」，設計目標為模組化與可擴展。

### GCamera（相機模組）

目前支援：
- Basler Camera（Pylon SDK）

尚未實作：
- Dalsa
- Hikvision（海康）

功能

- 相機開啟 / 關閉
- 開始 / 停止取像
- 影像回調事件

設計方向
- 統一相機控制介面
- 支援多品牌擴展


### IO（DIO / AIO 模組）

用於控制工業設備輸入輸出。

目前支援：
- 聚英 DAM0888（DIO）
- 聚英 DAM8DA（AIO）

功能
- DIO 點位讀寫
- AIO 類比輸出控制
- 整合 Modbus 通訊

應用
- 設備訊號控制（Busy / Ready / Alarm）
- AOI 結果輸出
- 外部設備觸發


## MotorController（馬達控制）

負責馬達控制與通訊管理。

目前支援：
- Oriental Motor（透過 Modbus RS485）

功能

- 馬達連線管理
- 移動控制（Move）
- 停止控制（Stop）
- 狀態刷新（位置 / 狀態）

設計方向
- 依通訊方式擴展（RS485 / Ethernet）
- 依品牌擴展（不同廠牌馬達）

應用
- AOI 載台移動
- 定位控制
- 自動化流程控制


## 設計目標

GUtility 並非單一應用程式，而是：

- 可重用模組集合
- AOI 專案基礎框架
- 自動化設備整合工具庫

讓不同專案可以：
- 快速組合功能
- 減少重寫設備控制
- 提升開發效率


## 開發環境
Language：C#
Framework：.NET Framework 4.7.2
IDE：Visual Studio 2015


## 專案狀態

### 目前已完成：
`Common.Log`
`Common.Recipe`
`Common.AutoPurge`
`Modbus（Server / Client 基礎）`
`GCamera（Basler）`
`IO（DAM0888 / DAM8DA）`
`MotorController（OrientalMotor RS485）`

### 持續開發中：
- Ini 功能
- 多品牌相機支援
- Modbus 通用 API
- 更多設備模組
- Example 使用範例


## License

MIT License

## License

MIT License

Copyright (c) 2026 GUtility

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

