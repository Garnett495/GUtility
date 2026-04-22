using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Ini.Abstractions;
using GUtility.Common.Ini.Core;
using GUtility.Common.Ini.Serialization;

namespace GUtility.Common.Ini.Example
{
    /// <summary>
    /// 設備設定物件範例。
    /// </summary>
    public class EquipmentConfigExample
    {
        public enum MachineMode
        {
            Offline = 0,
            Online = 1
        }

        public class MachineConfig
        {
            public string MachineId { get; set; }
            public string PlcIp { get; set; }
            public int PlcPort { get; set; }
            public double CameraExposure { get; set; }
            public bool AutoStart { get; set; }
            public MachineMode Mode { get; set; }
        }

        public void Run()
        {
            IGIniFile ini = new GIniFile(@"D:\Config\MachineConfig.ini");
            GIniObjectSerializer serializer = new GIniObjectSerializer();

            MachineConfig config = new MachineConfig();
            config.MachineId = "INX7_AOI_01";
            config.PlcIp = "192.168.0.10";
            config.PlcPort = 502;
            config.CameraExposure = 1200.0;
            config.AutoStart = true;
            config.Mode = MachineMode.Online;

            serializer.WriteObject(ini, config);

            MachineConfig loadedConfig = serializer.ReadObject<MachineConfig>(ini);
        }
    }
}
