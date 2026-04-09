# 📷 GUtility.Module.GCamera - BaslerCamera

## 功能概述
`BaslerCamera` 為 `GUtility.Module.GCamera` 中 Basler 相機的實作類別，封裝 `Basler pylon SDK`，提供 AOI 系統中常用的相機控制能力。

- 目前只實作`BaslerCamera` 
- `Dalsa`, `Hikvision` 尚未完成

主要目標：
- 統一相機操作介面（避免上層依賴 SDK）
- 提供穩定的取像流程（Thread-safe）
- 可與其他品牌（Dalsa / Hikvision）共用架構

---

## 功能特色 🚀
- 支援 `SerialNumber` 指定相機
- 支援連續取像（`StartGrabbing()`）
- 支援單張取像（`GrabOne()`）
- 支援軟體觸發（`SoftTrigger()`）
- 支援曝光 / 增益 / 幀率設定
- 自動轉換影像為 `CameraFrame`
- 背景執行緒抓圖（高效能）
- 與 UI 解耦（事件機制）

---

## 環境需求
使用前需安裝：

- `Basler pylon SDK`
- `Basler.Pylon.dll`

Pylon下載連結:
https://www.baslerweb.com/zh-tw/downloads/software/3922788174/

Basler Software 官網連結:
https://www.baslerweb.com/zh-tw/downloads/software/

⚠️ 注意：
- ❌ 無法透過 NuGet 安裝
- ✔ 必須安裝完整 SDK（含 .NET API）
- ✔ 專案需設定為 `x64`

---

## 檔案位置
GUtility/Module/GCamera/Providers/Basler/BaslerCamera.cs


---

## 核心方法說明

### 相機管理
- `SearchAvailableCamera()`
  - 搜尋目前可用相機序號

- `Initialize(CameraConfig config)`
  - 初始化設定（包含 `Brand`）

- `Open()`
  - 依 `CameraConfig.SerialNumber` 開啟相機
  - 自動套用：
    - `TriggerMode`
    - `Exposure`
    - `Gain`
    - `FrameRate`

- `Close()`
  - 關閉相機並釋放資源

---

### 取像控制
- `StartGrabbing()`
  - 啟動背景抓圖執行緒
  - 進入連續取像模式

- `StopGrabbing()`
  - 停止抓圖執行緒與 StreamGrabber

- `GrabOne()`
  - 單張取像（回傳 `Bitmap`）
  - 適用測試 / UI 顯示

---

### Trigger 控制
- `SetTriggerMode(TriggerMode mode)`

支援：
- `Continuous`
- `Software`
- `Hardware`

- `SoftTrigger()`
  - 執行一次軟體觸發

---

### 參數設定
- `SetExposure(double value)`
- `SetGain(double value)`
- `SetFrameRate(double value)`

說明：
- 同步更新 `CameraConfig`
- 若相機已開啟則立即寫入

---

### 資訊讀取
- `GetCameraInfo()`

包含：
- `ModelName`
- `SerialNumber`
- `IpAddress`
- `MaxWidth / MaxHeight`
- `PixelFormat`

---

## 影像流程

### 連續取像流程
```
StartGrabbing()
↓
GrabLoop()
↓
RetrieveResult()
↓
ConvertToCameraFrame()
↓
RaiseImageGrabbed(frame)
```


---

## 影像轉換機制

### `ConvertToCameraFrame(IGrabResult result)`
- 將 Basler 影像轉為系統格式 `CameraFrame`

流程：
1. 建立 `byte[] buffer`
2. 使用 `GCHandle` 固定記憶體
3. 呼叫 `_converter.Convert()`
4. 轉為 `RGB8packed`
5. 回傳 `CameraFrame`

---

### `ConvertToBitmap(IGrabResult result)`
- 將影像轉為 `Bitmap`
- 用於 UI 顯示或測試

---

## 使用範例

```csharp
var camera = new BaslerCamera();

var config = new CameraConfig();
config.SerialNumber = "12345678";
config.Exposure = 10000;
config.Gain = 5;
config.FrameRate = 30;

camera.Initialize(config);
camera.Open();

// 訂閱影像事件
camera.ImageGrabbed += (frame) =>
{
    Console.WriteLine("Frame: " + frame.FrameNumber);
};

// 開始抓圖
camera.StartGrabbing();

// 停止抓圖
camera.StopGrabbing();

// 單張取像
var bmp = camera.GrabOne();

// 關閉
camera.Close();
```

## 設計架構
採用 Provider 架構：
```
GCamera
 ├── Core
 ├── Models
 ├── Enums
 ├── Providers
 │    ├── Basler
 │    ├── Dalsa
 │    └── Hikvision
 ```
 
## 設計重點
- SDK 隔離：上層不依賴 Basler.Pylon
- Thread-safe：使用 _syncLock
- 非同步抓圖：背景執行緒
- 事件驅動：RaiseImageGrabbed()
- Config 與 Runtime 分離

## 已知限制
- SetFrameRate() 不同機型支援不同
- Hardware Trigger 尚未細化 Line 設定
- GrabOne() 為同步操作（非高效模式）
- 目前固定輸出 RGB8packed

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
