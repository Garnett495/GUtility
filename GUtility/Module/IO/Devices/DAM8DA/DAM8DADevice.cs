using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Abstractions;
using GUtility.Module.IO.Base;
using GUtility.Module.IO.Enums;
using GUtility.Module.IO.Models;

namespace GUtility.Module.IO.Devices.DAM8DA
{
    /// <summary>
    /// 聚英 DAM8DA 類比輸出模組。
    /// </summary>
    public class DAM8DADevice : GAioDeviceBase
    {
        public DAM8DADevice(GModbusDeviceConfig config, IGModbusTransport transport) : base(config, transport)
        {
            if (config == null) throw new ArgumentNullException("config");

            DeviceName = config.DeviceName;
            DeviceType = GDeviceType.AIO;
            AoChannelCount = DAM8DARegisterMap.AoCount;
            AnalogRange = GAnalogRangeType.Voltage0To10;

            SetAoSnapshot(new double[AoChannelCount]);
        }

        /// <summary>
        /// 讀取 AO 快取，若設備支援回讀則同步更新。
        /// </summary>
        public override void Refresh()
        {
            if (!IsConnected)
                return;

            try
            {
                ushort[] rawValues = Transport.ReadHoldingRegisters(
                    ModbusConfig.SlaveId,
                    DAM8DARegisterMap.AoStartAddress,
                    DAM8DARegisterMap.AoCount);

                double[] aoValues = new double[rawValues.Length];
                int i;

                for (i = 0; i < rawValues.Length; i++)
                {
                    aoValues[i] = ConvertRawToVoltage(rawValues[i]);
                }

                SetAoSnapshot(aoValues);

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
        /// 寫入單一 AO 電壓值。
        /// </summary>
        public override void WriteAO(int channel, double value)
        {
            ValidateChannel(channel, AoChannelCount);

            if (!IsConnected)
                throw new InvalidOperationException("Device is not connected.");

            ValidateVoltage(value);

            try
            {
                ushort rawValue = ConvertVoltageToRaw(value);
                ushort address = (ushort)(DAM8DARegisterMap.AoStartAddress + channel);

                Transport.WriteSingleRegister(ModbusConfig.SlaveId, address, rawValue);

                double[] aoCache = ReadAllAO();
                aoCache[channel] = value;
                SetAoSnapshot(aoCache);

                UpdateStatus(string.Empty);
            }
            catch (Exception ex)
            {
                ConnectionState = GDeviceConnectionState.Error;
                UpdateStatus(ex.Message);
                throw;
            }
        }

        private void ValidateVoltage(double value)
        {
            if (value < DAM8DARegisterMap.VoltageMin || value > DAM8DARegisterMap.VoltageMax)
            {
                throw new ArgumentOutOfRangeException("value",
                    "AO voltage out of range: " + value.ToString("0.###"));
            }
        }

        private ushort ConvertVoltageToRaw(double voltage)
        {
            double ratio = (voltage - DAM8DARegisterMap.VoltageMin) /
                           (DAM8DARegisterMap.VoltageMax - DAM8DARegisterMap.VoltageMin);

            double raw = DAM8DARegisterMap.RawMin +
                         ratio * (DAM8DARegisterMap.RawMax - DAM8DARegisterMap.RawMin);

            if (raw < DAM8DARegisterMap.RawMin)
                raw = DAM8DARegisterMap.RawMin;

            if (raw > DAM8DARegisterMap.RawMax)
                raw = DAM8DARegisterMap.RawMax;

            return (ushort)Math.Round(raw, MidpointRounding.AwayFromZero);
        }

        private double ConvertRawToVoltage(ushort raw)
        {
            double ratio = (double)(raw - DAM8DARegisterMap.RawMin) /
                           (double)(DAM8DARegisterMap.RawMax - DAM8DARegisterMap.RawMin);

            double voltage = DAM8DARegisterMap.VoltageMin +
                             ratio * (DAM8DARegisterMap.VoltageMax - DAM8DARegisterMap.VoltageMin);

            return voltage;
        }
    }
}
