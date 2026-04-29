# 📷 GUtility.Module.GCamera

## 功能概述
`GCamera` 為 AOI 系統通用相機模組，提供統一的相機控制介面，並透過 Provider 架構支援不同品牌相機。

目前支援：
- ✔ `BaslerCamera`
- ✔ `HikCamera`（第一版）
- ⏳ `Dalsa`（規劃中）

主要目標：
- 統一相機操作介面（避免上層依賴 SDK）
- 提供穩定取像流程（Thread-safe）
- 支援多品牌擴展

---

## 功能特色 🚀
- 支援 `SerialNumber` 指定相機
- 支援連續取像 `StartGrabbing()`
- 支援單張取像 `GrabOne()`（Basler）
- 支援軟體觸發 `SoftTrigger()`
- 支援曝光 / 增益 / 幀率設定
- 影像統一轉為 `CameraFrame`
- 背景執行緒抓圖
- 事件驅動（`ImageGrabbed`）

---

## 支援相機

### Basler
- SDK：`Basler pylon SDK`
- DLL：`Basler.Pylon.dll`

下載：
- https://www.baslerweb.com/zh-tw/downloads/software/

---

### Hikvision (Hikrobot)
- SDK：`MVS SDK`
- DLL：`MvCameraControl.Net.dll`

下載：
- https://www.hikrobotics.com/en/machinevision/service/download/

---

## 環境需求
- .NET Framework `4.7.2`
- Platform：`x64`

⚠️ 注意：
- ❌ 不支援 NuGet 安裝
- ✔ 必須安裝完整 SDK（含 driver / runtime）

---

## 架構設計
採用 Provider 架構：
```
GCamera
├── Core
├── Models
├── Enums
├── Events
├── Providers
│ ├── Basler
│ ├── Hikvision
│ └── Dalsa
```

---

## 核心方法

### 相機管理
- `Initialize(CameraConfig config)`
- `Open()`
- `Close()`

---

### 取像控制
- `StartGrabbing()`
- `StopGrabbing()`
- `GrabOne()`（Basler）

---

### Trigger 控制
- `SetTriggerMode(TriggerMode mode)`
- `SoftTrigger()`

---

### 參數設定
- `SetExposure(double value)`
- `SetGain(double value)`
- `SetFrameRate(double value)`

---

### 資訊取得
- `GetCameraInfo()`

---

## 影像流程
```
StartGrabbing()
↓
GrabLoop()
↓
( SDK 取像 )
↓
ConvertToCameraFrame()
↓
RaiseImageGrabbed(frame)
```

---

## 使用範例

```csharp
CameraManager manager = new CameraManager();

CameraConfig config = new CameraConfig();
config.CameraId = "CAM1";
config.Brand = CameraBrand.Hikvision; // 或 Basler
config.SerialNumber = "YOUR_SN";

manager.AddCamera(config);

var cam = manager.GetCamera("CAM1");

cam.ImageGrabbed += (sender, e) =>
{
    Console.WriteLine($"Frame: {e.Frame.FrameNumber}");
};

cam.Open();
cam.StartGrabbing();

Console.ReadKey();

cam.StopGrabbing();
cam.Close();
```


## 設計重點
- SDK 隔離（上層不依賴 Basler.Pylon / MvCameraControl）
- Thread-safe（_syncLock）
- 非同步抓圖（Background Thread）
- 事件驅動（ImageGrabbed）
- Config 與 Runtime 分離


## 已知限制
- SetFrameRate() 不同機型支援不同
- Hardware Trigger 尚未完整實作
- Hikvision 目前使用 Bitmap 轉換（效能可優化）
- 尚未實作自動重連（Reconnect）


## 後續規劃
- Reconnect 機制
- Buffer Pool（降低 GC）
- Bayer → RGB 最佳化
- 多相機同步控制
- Trigger Line 完整設定



## License

MIT License
Copyright (c) 2026 Garnett.C