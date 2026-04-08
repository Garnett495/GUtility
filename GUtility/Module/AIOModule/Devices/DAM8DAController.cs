using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.Devices
{
    public class DAM8DAController : ModbusAODeviceBase
    {
        private const int CHANNEL_COUNT = 8;
        private const int AO_START_OFFSET = 0;   // 4x0001 -> offset 0

        public DAM8DAController(string ip, int port, byte slaveId) : base(ip, port, slaveId)
        {
        }

        public override void SetChannelRaw(int channel, ushort rawValue)
        {
            ValidateChannel(channel);

            int offset = AO_START_OFFSET + (channel - 1);
            WriteHoldingRegister(offset, rawValue);
        }

        public override void SetChannelValue(int channel, double actualValue)
        {
            ushort rawValue = ConvertActualValueToRaw(actualValue);
            SetChannelRaw(channel, rawValue);
        }

        public override void SetChannelsRaw(ushort[] rawValues)
        {
            if (rawValues == null)
                throw new ArgumentNullException("rawValues");

            int count = rawValues.Length;
            if (rawValues.Length == 0)
                return;
            if (rawValues.Length > CHANNEL_COUNT)
                throw new ArgumentException("rawValues length cannot exceed 8.", "rawValues");

            for (int i = 0; i < count; i++)
            {
                SetChannelRaw(i + 1, rawValues[i]);
            }
        }

        public override void ResetAll()
        {
            for (int ch = 1; ch <= CHANNEL_COUNT; ch++)
            {
                SetChannelRaw(ch, 0);
            }
        }

        private void ValidateChannel(int channel)
        {
            if (channel < 1 || channel > CHANNEL_COUNT)
                throw new ArgumentOutOfRangeException("channel", "channel must be 1~8.");
        }

        private ushort ConvertActualValueToRaw(double actualValue)
        {
            // 寫入值 = 實際值 * 100
            // 例如 4.00 -> 400, 10.00 -> 1000
            int raw = (int)Math.Round(actualValue * 100.0, MidpointRounding.AwayFromZero);

            if (raw < 0)
                raw = 0;

            if (raw > ushort.MaxValue)
                raw = ushort.MaxValue;

            return (ushort)raw;
        }
    }
}
