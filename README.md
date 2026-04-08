# BaslerCamera 功能說明

## 1. 功能概述
`BaslerCamera` 為 `GUtility.Module.GCamera` 之 Basler 相機實作類別，負責封裝 Basler.Pylon SDK 的基本相機控制功能。

目前支援：
- 依相機序號（SerialNumber）開啟指定相機
- 搜尋可用 Basler 相機
- 開啟 / 關閉相機
- 開始 / 停止連續取像
- 軟體觸發（Software Trigger）
- 單張取像（GrabOne）
- 曝光、增益、幀率設定
- 相機資訊讀取
- 將 Basler 影像結果轉為系統內部 `CameraFrame`

---

## 2. 檔案與注意事項
GUtility/Module/GCamera/Providers/Basler/BaslerCamera.cs

需先安裝 `Basler pylon SDK`，並於專案中加入：
`Basler.Pylon.dll`

注意：

不可只用 NuGet 安裝
需安裝完整 pylon SDK 與 .NET API
專案平台建議使用 x64

4. 主要方法說明
`SearchAvailableCamera()`

搜尋目前可用的 Basler 相機序號清單。

`Initialize(CameraConfig config)`

初始化相機設定，並將 Brand 指定為 Basler。

`Open()`

依 CameraConfig.SerialNumber 搜尋並開啟指定相機，成功後自動套用初始化參數。

`Close()`

停止抓圖、關閉相機並釋放 SDK 資源。

`StartGrabbing()`

啟動背景抓圖執行緒，進入連續取像模式。

`StopGrabbing()`

停止背景抓圖執行緒與 StreamGrabber。

`SoftTrigger()`

執行一次 Basler 軟體觸發。

`SetTriggerMode(TriggerMode mode)`

設定取像模式：

`Continuous`
`Software`
`Hardware`


`SetExposure(double value)`

設定曝光時間。

`SetGain(double value)`

設定增益。

`SetFrameRate(double value)`

設定幀率；若機型節點不支援，則僅保留設定值。

`GetCameraInfo()`

讀取目前相機的基本資訊，例如：

型號
序號
IP
最大解析度
PixelFormat

`GrabOne()`

單張取像並回傳 Bitmap。

5. 影像事件流程

`StartGrabbing()` 後會啟動背景執行緒 `GrabLoop()`，當抓到影像時會：

將 `IGrabResult` 轉為 `CameraFrame`
呼叫 `RaiseImageGrabbed(frame)`
由上層模組接收影像事件並處理

6. 內部轉換流程
`ConvertToCameraFrame(IGrabResult result)`

將 Basler 影像結果轉為系統內部 `CameraFrame`：

轉為 RGB8packed
存入 byte[] Buffer
記錄 Width / Height / Timestamp / FrameNumber

`ConvertToBitmap(IGrabResult result)`

將 Basler 影像結果轉為 Bitmap，主要供單張取像或測試顯示使用。

7. 設計重點
- Basler SDK 操作集中於 Provider 層
- 上層不直接依賴 Basler SDK 型別
- 抓圖執行緒與相機控制以 _syncLock 保護
- `StopGrabbing()` 與 `Close()` 責任分離：
- `StopGrabbing()`：只停抓圖
- `Close()`：停抓圖 + 關閉相機 + 釋放資源

8. 已知限制
- `SetFrameRate()` 依不同 Basler 型號，節點支援可能不同
- `Hardware Trigger` 目前僅設定 TriggerMode，尚未額外細分 TriggerSource/Line
- `GrabOne()` 回傳 Bitmap，主要供測試與工程模式使用
- 目前 `ConvertToCameraFrame()` 固定輸出為 RGB8packed