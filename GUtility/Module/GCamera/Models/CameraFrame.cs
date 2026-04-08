using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.GCamera.Models
{
    /// <summary>
    /// 單張影像資料
    /// </summary>
    public class CameraFrame
    {
        public string CameraId { get; set; }
        public DateTime Timestamp { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public byte[] Buffer { get; set; }

        public string PixelFormat { get; set; }
        public long FrameNumber { get; set; }

        public CameraFrame()
        {
            CameraId = string.Empty;
            Timestamp = DateTime.Now;
            Width = 0;
            Height = 0;
            Buffer = null;
            PixelFormat = string.Empty;
            FrameNumber = 0;
        }
    }
}
