using GUtility.Module.GCamera.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Module.GCamera.Models
{
    /// <summary>
    /// 相機使用設定
    /// </summary>
    public class CameraConfig
    {
        public string CameraId { get; set; }
        public string CameraName { get; set; }
        public CameraBrand Brand { get; set; }

        public string IpAddress { get; set; }
        public string SerialNumber { get; set; }

        public TriggerMode TriggerMode { get; set; }

        public double Exposure { get; set; }
        public double Gain { get; set; }
        public double FrameRate { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public int TimeoutMs { get; set; }

        public CameraConfig()
        {
            CameraId = string.Empty;
            CameraName = string.Empty;
            IpAddress = string.Empty;
            SerialNumber = string.Empty;

            Brand = CameraBrand.Unknown;
            TriggerMode = TriggerMode.Continuous;

            Exposure = 0;
            Gain = 0;
            FrameRate = 0;

            Width = 0;
            Height = 0;

            TimeoutMs = 3000;
        }
    }
}
