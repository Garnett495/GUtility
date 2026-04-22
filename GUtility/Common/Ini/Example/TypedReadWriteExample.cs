using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Ini.Core;

namespace GUtility.Common.Ini.Example
{
    /// <summary>
    /// 型別讀寫範例。
    /// </summary>
    public class TypedReadWriteExample
    {
        private enum CameraBrand
        {
            Unknown = 0,
            Basler = 1,
            Hikvision = 2,
            Dalsa = 3
        }

        public void Run()
        {
            GIniFile ini = new GIniFile(@"D:\Config\Equipment.ini");

            ini.WriteDouble("Camera", "ExposureTime", 1500.5);
            ini.WriteDouble("Camera", "Gain", 1.25);
            ini.WriteEnum("Camera", "Brand", CameraBrand.Basler);
            ini.WriteBool("Camera", "EnableTrigger", true);
            ini.Save();

            double exposure = ini.ReadDouble("Camera", "ExposureTime", 1000);
            double gain = ini.ReadDouble("Camera", "Gain", 1.0);
            CameraBrand brand = ini.ReadEnum("Camera", "Brand", CameraBrand.Unknown);
            bool enableTrigger = ini.ReadBool("Camera", "EnableTrigger", false);
        }
    }
}
