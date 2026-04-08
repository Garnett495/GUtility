using GUtility.Module.GCamera.Core;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.GCamera.Providers.Dalsa
{
    /// <summary>
    /// Dalsa 相機骨架
    /// 目前先不綁 SDK，先把架構跑起來
    /// </summary>
    public class DalsaCamera : CameraBase
    {
        public override CameraBrand Brand
        {
            get { return CameraBrand.Dalsa; }
        }

        public override bool Open()
        {
            if (!IsInitialized)
                return false;

            try
            {
                // TODO: 在這裡補 Dalsa SDK 開啟相機
                _status.IsOpen = true;
                _status.LastUpdateTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa Open failed.", ex);
                return false;
            }
        }

        public override bool Close()
        {
            try
            {
                // TODO: 在這裡補 Dalsa SDK 關閉相機
                _status.IsGrabbing = false;
                _status.IsOpen = false;
                _status.LastUpdateTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa Close failed.", ex);
                return false;
            }
        }

        public override bool StartGrabbing()
        {
            if (!IsOpen)
                return false;

            try
            {
                // TODO: 在這裡補 Dalsa SDK 開始取像
                _status.IsGrabbing = true;
                _status.LastUpdateTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa StartGrabbing failed.", ex);
                return false;
            }
        }

        public override bool StopGrabbing()
        {
            try
            {
                // TODO: 在這裡補 Dalsa SDK 停止取像
                _status.IsGrabbing = false;
                _status.LastUpdateTime = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa StopGrabbing failed.", ex);
                return false;
            }
        }

        public override bool SoftTrigger()
        {
            if (!IsOpen)
                return false;

            try
            {
                // TODO: 在這裡補 Dalsa SDK 軟觸發
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa SoftTrigger failed.", ex);
                return false;
            }
        }

        public override bool SetTriggerMode(TriggerMode mode)
        {
            try
            {
                // TODO: 在這裡補 Dalsa SDK 觸發模式設定
                _config.TriggerMode = mode;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa SetTriggerMode failed.", ex);
                return false;
            }
        }

        public override bool SetExposure(double value)
        {
            try
            {
                // TODO: 在這裡補 Dalsa SDK 曝光設定
                _config.Exposure = value;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa SetExposure failed.", ex);
                return false;
            }
        }

        public override bool SetGain(double value)
        {
            try
            {
                // TODO: 在這裡補 Dalsa SDK Gain 設定
                _config.Gain = value;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa SetGain failed.", ex);
                return false;
            }
        }

        public override bool SetFrameRate(double value)
        {
            try
            {
                // TODO: 在這裡補 Dalsa SDK FrameRate 設定
                _config.FrameRate = value;
                return true;
            }
            catch (Exception ex)
            {
                RaiseError("Dalsa SetFrameRate failed.", ex);
                return false;
            }
        }

        public override CameraInfo GetCameraInfo()
        {
            CameraInfo info = new CameraInfo();
            info.CameraId = CameraId;
            info.CameraName = CameraName;
            info.Brand = Brand;
            info.SerialNumber = _config != null ? _config.SerialNumber : string.Empty;
            info.IpAddress = _config != null ? _config.IpAddress : string.Empty;
            info.VendorName = "Dalsa";
            info.ModelName = "Unknown";
            return info;
        }
    }
}
