using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module
{
    public interface IAODeviceController : IDisposable
    {
        bool IsConnected { get; }

        bool Connect();

        void Disconnect();

        void SetChannelRaw(int channel, ushort rawValue);
        void SetChannelValue(int channel, double actualValue);
        void SetChannelsRaw(ushort[] rawValues);
        void ResetAll();
    }
}
