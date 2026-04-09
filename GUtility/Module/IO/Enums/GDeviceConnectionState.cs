using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.IO.Enums
{
    /// <summary>
    /// 裝置連線狀態。
    /// </summary>
    public enum GDeviceConnectionState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Error = 3
    }
}
