## GUtility.Common.Network

### 功能簡介 🚀

提供簡單且穩定的網路介面卡控制工具，支援在設備異常（如 GigE 相機斷線）時，自動停用並重新啟用指定網卡，用於快速恢復連線能力。

---

## 功能特色 ✨

* 🔄 自動重啟指定網路介面卡
* ⚙️ 可設定停用 / 啟用等待時間
* 🧩 模組化設計，可獨立於相機或設備使用
* 🧵 Thread-safe 呼叫（適用多執行緒環境）
* 🔌 適用 AOI / 自動化設備 / GigE Camera

---

## 架構設計

```text
GNetworkAdapterRestarter
    ↓
GNetworkAdapterRestartOptions
    ↓
GNetworkAdapterRestartResult
```

---

## 使用方法

### 1. 建立設定

```csharp
var options = new GNetworkAdapterRestartOptions
{
    AdapterName = "乙太網路 2",
    DisableWaitMs = 5000,
    EnableWaitMs = 5000
};
```

---

### 2. 執行重啟

```csharp
var restarter = new GNetworkAdapterRestarter();

var result = restarter.Restart(options);

if (!result.Success)
{
    Console.WriteLine(result.Message);
}
```

---

## 使用情境

* 相機斷線後無法重新連線
* GigE 相機網卡資源被占用
* AOI 設備自動復歸流程

---

## 注意事項

* 必須以系統管理員權限執行
* `AdapterName` 需對應 Windows 網路介面名稱
* 建議搭配相機重新連線機制使用（而非單獨使用）

---

## 建議最佳流程

```text
1. StopGrab()
2. Close Camera
3. Restart Network Adapter
4. Open Camera
5. StartGrab()
```

---

## Example

```csharp
NetworkAdapterRestartExample.RunBasicExample();
```
