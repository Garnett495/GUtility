using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Module.GCamera.Events
{
    /// <summary>
    /// 影像事件參數
    /// </summary>
    public class CameraImageEventArgs : EventArgs
    {
        public CameraFrame Frame { get; private set; }

        public CameraImageEventArgs(CameraFrame frame)
        {
            Frame = frame;
        }
    }
}
