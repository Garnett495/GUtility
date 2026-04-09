using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.IO.Models
{
    /// <summary>
    /// Modbus TCP 裝置設定。
    /// </summary>
    public class GModbusDeviceConfig : GDeviceConfig
    {
        /// <summary>
        /// 裝置 IP。
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 通訊埠。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Slave ID。
        /// </summary>
        public byte SlaveId { get; set; }

        /// <summary>
        /// 通訊逾時毫秒。
        /// </summary>
        public int TimeoutMs { get; set; }

        public GModbusDeviceConfig()
        {
            IP = "127.0.0.1";
            Port = 502;
            SlaveId = 1;
            TimeoutMs = 1000;
        }
    }
}
