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

## 2. 檔案位置
```text
GUtility/Module/GCamera/Providers/Basler/BaslerCamera.cs