using GUtility.Module.GCamera.Abstraction;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Events;
using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Module.GCamera.Core
{
    /// <summary>
    /// 相機共用基底類別
    /// </summary>
    public abstract class CameraBase : ICamera
    {
        protected CameraConfig _config;
        protected CameraStatus _status;

        public string CameraId
        {
            get { return _config != null ? _config.CameraId : string.Empty; }
        }

        public string CameraName
        {
            get { return _config != null ? _config.CameraName : string.Empty; }
        }

        public abstract CameraBrand Brand { get; }

        public bool IsInitialized
        {
            get { return _status != null && _status.IsInitialized; }
        }

        public bool IsOpen
        {
            get { return _status != null && _status.IsOpen; }
        }

        public bool IsGrabbing
        {
            get { return _status != null && _status.IsGrabbing; }
        }

        public event EventHandler<CameraImageEventArgs> ImageGrabbed;
        public event EventHandler<CameraErrorEventArgs> ErrorOccurred;

        protected CameraBase()
        {
            _status = new CameraStatus();
        }

        public virtual bool Initialize(CameraConfig config)
        {
            if (config == null)
                return false;

            _config = config;
            _status.IsInitialized = true;
            _status.LastUpdateTime = DateTime.Now;
            return true;
        }

        public abstract bool Open();
        public abstract bool Close();
        public abstract bool StartGrabbing();
        public abstract bool StopGrabbing();
        public abstract bool SoftTrigger();
        public abstract bool SetTriggerMode(TriggerMode mode);
        public abstract bool SetExposure(double value);
        public abstract bool SetGain(double value);
        public abstract bool SetFrameRate(double value);
        public abstract CameraInfo GetCameraInfo();

        public virtual CameraStatus GetStatus()
        {
            return _status;
        }

        protected void RaiseImageGrabbed(CameraFrame frame)
        {
            if (ImageGrabbed != null)
                ImageGrabbed(this, new CameraImageEventArgs(frame));
        }

        protected void RaiseError(string message, Exception ex)
        {
            _status.LastErrorMessage = message;
            _status.LastUpdateTime = DateTime.Now;

            if (ErrorOccurred != null)
                ErrorOccurred(this, new CameraErrorEventArgs(message, ex));
        }

        public virtual void Dispose()
        {
            try
            {
                if (IsGrabbing)
                    StopGrabbing();

                if (IsOpen)
                    Close();
            }
            catch
            {
                // Dispose 階段不往外丟例外，避免關閉流程中斷
            }
        }
    }
}
