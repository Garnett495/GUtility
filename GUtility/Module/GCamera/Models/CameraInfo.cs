using GUtility.Module.GCamera.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.GCamera.Models
{
    /// <summary>
    /// 相機設備資訊
    /// </summary>
    public class CameraInfo
    {
        public string CameraId { get; set; }
        public string CameraName { get; set; }

        public CameraBrand Brand { get; set; }

        public string VendorName { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public string IpAddress { get; set; }

        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        public string PixelFormat { get; set; }

        public CameraInfo()
        {
            CameraId = string.Empty;
            CameraName = string.Empty;
            VendorName = string.Empty;
            ModelName = string.Empty;
            SerialNumber = string.Empty;
            IpAddress = string.Empty;
            PixelFormat = string.Empty;

            Brand = CameraBrand.Unknown;

            MaxWidth = 0;
            MaxHeight = 0;
        }
    }
}
