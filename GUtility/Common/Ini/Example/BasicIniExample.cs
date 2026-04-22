using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Ini.Core;

namespace GUtility.Common.Ini.Example
{
    /// <summary>
    /// 基本 Ini 讀寫範例。
    /// </summary>
    public class BasicIniExample
    {
        public void Run()
        {
            GIniFile ini = new GIniFile(@"D:\Config\Machine.ini");

            ini.WriteString("Machine", "MachineName", "AOI-01");
            ini.WriteString("PLC", "Ip", "192.168.0.10");
            ini.WriteInt("PLC", "Port", 502);
            ini.WriteBool("System", "EnableDebugLog", true);

            ini.Save();

            string machineName = ini.ReadString("Machine", "MachineName", "Unknown");
            string plcIp = ini.ReadString("PLC", "Ip", "127.0.0.1");
            int plcPort = ini.ReadInt("PLC", "Port", 502);
            bool enableDebug = ini.ReadBool("System", "EnableDebugLog", false);
        }
    }
}
