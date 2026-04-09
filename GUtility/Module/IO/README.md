📦 GUtility.Module.IO

## 簡介
一套可擴展的工業設備 I/O 控制模組，支援 DIO / AIO / Modbus 架構設計，適用於 AOI、自動化設備整合等場景。

## 功能特色
```
🔌 支援 Modbus TCP 通訊架構
⚙️ 可同時控制多個 I/O 模組
🧱 模組化設計（DIO / AIO 分離）
🔄 Snapshot 快取機制（避免頻繁通訊）
🧵 Thread-safe 設計（支援多執行緒）
🔁 易於擴展（未來可加入其他設備）
🔧 支援不同設備（如 DAM0888、DAM8DA）
```

## 架構設計
GDeviceBase
    ↓
GModbusDeviceBase
    ↓
├── GDioDeviceBase   → DIO設備
└── GAioDeviceBase   → AIO設備


## 設計概念
1. 分層架構
GDeviceBase
→ 管理設備生命週期（Connect / Disconnect / Dispose）

- GModbusDeviceBase
→ 負責 Modbus 通訊管理

- GDioDeviceBase / GAioDeviceBase
→ 負責 I/O 行為與快取

2. Snapshot 快取機制
- 所有 `Read` 操作 不直接將訊號傳至設備
- 透過 `Refresh()` 更新資料
- 提升效能 + 降低通訊壓力

3. Transport 抽象
- 使用 IGModbusTransport
- 可替換：
	TCP
	RTU
	Mock（測試用）


## 資料夾結構
```
GUtility.Module.IO
│
├── Abstractions
│   ├── IGDevice.cs
│   ├── IGDioDevice.cs
│   ├── IGAioDevice.cs
│   ├── IGModbusDevice.cs
│   └── IGModbusTransport.cs
│
├── Base
│   ├── GDeviceBase.cs
│   ├── GModbusDeviceBase.cs
│   ├── GDioDeviceBase.cs
│   └── GAioDeviceBase.cs
│
├── Devices
│   ├── DAM0888.cs
│   └── DAM8DA.cs
│
├── Enums
│   ├── GDeviceType.cs
│   ├── GDeviceConnectionState.cs
│   └── GAnalogRangeType.cs
│
├── Models
│   ├── GModbusDeviceConfig.cs
│   ├── GIoSnapshot.cs
│   └── GDeviceStatus.cs
│
└── Transport
    └── ModbusTcpTransport.cs
```


## 使用範例
1. 建立 Modbus 設定
var config = new GModbusDeviceConfig()
{
    DeviceName = "DAM0888_1",
    DeviceType = GDeviceType.DIO,
    IP = "192.168.1.100",
    Port = 502
};

2. 建立 Transport
IGModbusTransport transport = new ModbusTcpTransport();

3. 建立 DIO 裝置（DAM0888）
var dio = new DAM0888(config, transport);

3. 連線設備
bool isConnected = dio.Connect();

4. 更新資料（重要）
dio.Refresh();

5. 讀取 DI / DO
bool di0 = dio.ReadDI(0);
bool do0 = dio.ReadDO(0);

6. 控制 DO
dio.WriteDO(0, true);

7. 讀取全部
bool[] allDI = dio.ReadAllDI();
bool[] allDO = dio.ReadAllDO();

## 使用注意事項
- 必須呼叫 `Refresh()`
dio.Refresh();

否則：
`ReadDI()` / `ReadDO()` 會拿到舊資料

- Thread-safe
所有讀取已內建lock可安全用於多執行緒

- 不建議：
每次Read都去Modbus讀

- 建議：
定時Refresh + Read快取

## 擴展新設備方式

以新增一個新設備為例：

1. 繼承 `Base`
public class MyDioDevice : GDioDeviceBase

2. 實作 `Refresh()`
public override void Refresh()
{
    var di = Transport.ReadDiscreteInputs(...);
    var do = Transport.ReadCoils(...);

    SetDiSnapshot(di);
    SetDoSnapshot(do);
}
3. 實作 `WriteDO()`
public override void WriteDO(int channel, bool value)
{
    Transport.WriteSingleCoil(...);
}


## 未來擴展方向
 IO Manager（自動輪詢）
 Event-driven（DI變化通知）
 Log 系統整合
 支援 Modbus RTU
 UI 控制模組

- 適用場景
AOI 設備控制
自動化產線 I/O 控制
PLC / IO 模組整合
工業設備監控系統

-  設計理念（重點）
解耦（Device vs Transport）
高可維護性
高可擴展性
避免重工（Reusable Module）

## License

MIT License