using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Abstractions;
using GUtility.Module.IO.Communication;
using GUtility.Module.IO.Devices.DAM0888;
using GUtility.Module.IO.Devices.DAM8DA;
using GUtility.Module.IO.Enums;
using GUtility.Module.IO.Management;
using GUtility.Module.IO.Models;

namespace GUtility.Module.IO.Example
{
    internal class SimpleUseSample
    {

        public void Run()
        {
            // 建立 Manager
            GIoManager ioManager = new GIoManager();

            // DAM0888 設定
            GModbusDeviceConfig dam0888Config = new GModbusDeviceConfig();
            dam0888Config.DeviceName = "DAM0888_1";
            dam0888Config.DeviceType = GDeviceType.DIO;
            dam0888Config.IP = "192.168.1.10";
            dam0888Config.Port = 502;
            dam0888Config.SlaveId = 1;

            // DAM8DA 設定
            GModbusDeviceConfig dam8daConfig = new GModbusDeviceConfig();
            dam8daConfig.DeviceName = "DAM8DA_1";
            dam8daConfig.DeviceType = GDeviceType.AIO;
            dam8daConfig.IP = "192.168.1.11";
            dam8daConfig.Port = 502;
            dam8daConfig.SlaveId = 1;

            // 建立裝置
            IGDioDevice dioDevice = new DAM0888Device(dam0888Config, new ModbusTcpTransport());
            IGAioDevice aioDevice = new DAM8DADevice(dam8daConfig, new ModbusTcpTransport());

            // 加入管理器
            ioManager.AddDevice(dioDevice);
            ioManager.AddDevice(aioDevice);

            // 連線
            ioManager.ConnectAll();

            // 更新快取
            ioManager.RefreshAll();

            // 寫 DO
            dioDevice.WriteDO(0, true);

            // 讀 DI
            bool di0 = dioDevice.ReadDI(0);

            // 寫 AO 5V
            aioDevice.WriteAO(0, 5.0);

            // 讀 AO 快取
            double ao0 = aioDevice.ReadAO(0);
        }

    }
}
