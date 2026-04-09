using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Abstractions;
using GUtility.Module.IO.Base;
using GUtility.Module.IO.Enums;
using GUtility.Module.IO.Models;

namespace GUtility.Module.IO.Devices.DAM0888
{
    /// <summary>
    /// 聚英 DAM0888 數位 I/O 模組。
    /// </summary>
    public class DAM0888Device : GDioDeviceBase
    {
        public DAM0888Device(GModbusDeviceConfig config, IGModbusTransport transport)
            : base(config, transport)
        {
            if (config == null) throw new ArgumentNullException("config");

            DeviceName = config.DeviceName;
            DeviceType = GDeviceType.DIO;
            DiChannelCount = DAM0888RegisterMap.DiCount;
            DoChannelCount = DAM0888RegisterMap.DoCount;

            SetDiSnapshot(new bool[DiChannelCount]);
            SetDoSnapshot(new bool[DoChannelCount]);
        }

        /// <summary>
        /// 重新讀取 DI / DO 狀態並更新快取。
        /// </summary>
        public override void Refresh()
        {
            if (!IsConnected)
                return;

            try
            {
                bool[] diValues = Transport.ReadInputs(
                    ModbusConfig.SlaveId,
                    DAM0888RegisterMap.DiStartAddress,
                    DAM0888RegisterMap.DiCount);

                bool[] doValues = Transport.ReadCoils(
                    ModbusConfig.SlaveId,
                    DAM0888RegisterMap.DoStartAddress,
                    DAM0888RegisterMap.DoCount);

                SetDiSnapshot(diValues);
                SetDoSnapshot(doValues);

                ConnectionState = GDeviceConnectionState.Connected;
                UpdateStatus(string.Empty);
            }
            catch (Exception ex)
            {
                ConnectionState = GDeviceConnectionState.Error;
                UpdateStatus(ex.Message);
            }
        }

        /// <summary>
        /// 寫入單一 DO。
        /// </summary>
        public override void WriteDO(int channel, bool value)
        {
            ValidateChannel(channel, DoChannelCount, "DO");

            if (!IsConnected)
            {
                throw new InvalidOperationException("Device is not connected.");
            }

            try
            {
                ushort address = (ushort)(DAM0888RegisterMap.DoStartAddress + channel);
                Transport.WriteSingleCoil(ModbusConfig.SlaveId, address, value);

                bool[] doCache = ReadAllDO();
                doCache[channel] = value;
                SetDoSnapshot(doCache);

                UpdateStatus(string.Empty);
            }
            catch (Exception ex)
            {
                ConnectionState = GDeviceConnectionState.Error;
                UpdateStatus(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 一次寫入所有 DO。
        /// </summary>
        public override void WriteAllDO(bool[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");

            if (values.Length != DoChannelCount)
                throw new ArgumentException("DO values length mismatch.");

            if (!IsConnected)
                throw new InvalidOperationException("Device is not connected.");

            try
            {
                Transport.WriteMultipleCoils(
                    ModbusConfig.SlaveId,
                    DAM0888RegisterMap.DoStartAddress,
                    values);

                SetDoSnapshot((bool[])values.Clone());
                UpdateStatus(string.Empty);
            }
            catch (Exception ex)
            {
                ConnectionState = GDeviceConnectionState.Error;
                UpdateStatus(ex.Message);
                throw;
            }
        }
    }
}
