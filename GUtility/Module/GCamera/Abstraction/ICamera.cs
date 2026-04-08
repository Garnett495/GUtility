using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Events;
using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.GCamera.Abstraction
{
    /// <summary>
    /// 相機統一介面
    /// </summary>
    public interface ICamera : IDisposable
    {
        string CameraId { get; }
        string CameraName { get; }
        CameraBrand Brand { get; }

        bool IsInitialized { get; }
        bool IsOpen { get; }
        bool IsGrabbing { get; }

        event EventHandler<CameraImageEventArgs> ImageGrabbed;
        event EventHandler<CameraErrorEventArgs> ErrorOccurred;

        bool Initialize(CameraConfig config);
        bool Open();
        bool Close();

        bool StartGrabbing();
        bool StopGrabbing();

        bool SoftTrigger();
        bool SetTriggerMode(TriggerMode mode);

        bool SetExposure(double value);
        bool SetGain(double value);
        bool SetFrameRate(double value);

        CameraInfo GetCameraInfo();
        CameraStatus GetStatus();
    }
}
