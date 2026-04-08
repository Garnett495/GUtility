using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.GCamera.Models
{
    /// <summary>
    /// 相機目前狀態
    /// </summary>
    public class CameraStatus
    {
        public bool IsInitialized { get; set; }
        public bool IsOpen { get; set; }
        public bool IsGrabbing { get; set; }

        public string LastErrorMessage { get; set; }
        public DateTime LastUpdateTime { get; set; }

        public CameraStatus()
        {
            IsInitialized = false;
            IsOpen = false;
            IsGrabbing = false;
            LastErrorMessage = string.Empty;
            LastUpdateTime = DateTime.Now;
        }
    }
}
