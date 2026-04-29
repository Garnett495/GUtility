using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

using MvCameraControl;

using GUtility.Module.GCamera.Core;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Models;


namespace GUtility.Module.GCamera.Providers.Hikvision
{
    /// <summary>
    /// Hikvision / Hikrobot 相機實作類別。
    /// 使用 MvCameraControl.Net.dll 的 IDevice 架構。
    /// </summary>
    public class HikCamera : CameraBase
    {
        private readonly object _syncLock = new object();

        private IDevice _device;
        private IDeviceInfo _deviceInfo;
        private Thread _grabThread;
        private volatile bool _grabThreadRunning;
        private long _frameNumber;

        private const int RGB_CHANNEL_COUNT = 3;

        public override CameraBrand Brand
        {
            get { return CameraBrand.Hikvision; }
        }

        public override bool Initialize(CameraConfig config)
        {
            if (!base.Initialize(config))
                return false;

            _config.Brand = CameraBrand.Hikvision;
            return true;
        }

        public override bool Open()
        {
            lock (_syncLock)
            {
                try
                {
                    if (!IsInitialized)
                        return false;

                    if (_device != null && _status.IsOpen)
                        return true;

                    _deviceInfo = FindDeviceInfo();
                    if (_deviceInfo == null)
                        throw new Exception("Hikvision camera not found. SerialNumber = " + _config.SerialNumber);

                    _device = DeviceFactory.CreateDevice(_deviceInfo);

                    int ret = _device.Open();
                    if (!IsSuccess(ret))
                        throw new Exception("Open device failed. ErrorCode = " + ret);

                    ApplyOpenParameters();

                    _status.IsOpen = true;
                    _status.IsGrabbing = false;
                    _status.LastErrorMessage = string.Empty;
                    _status.LastUpdateTime = DateTime.Now;

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision Open failed.", ex);
                    return false;
                }
            }
        }

        public override bool Close()
        {
            lock (_syncLock)
            {
                try
                {
                    StopGrabbingInternal();

                    if (_device != null)
                    {
                        try { _device.Close(); } catch { }
                        try { _device.Dispose(); } catch { }
                        _device = null;
                    }

                    _status.IsOpen = false;
                    _status.IsGrabbing = false;
                    _status.LastUpdateTime = DateTime.Now;

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision Close failed.", ex);
                    return false;
                }
            }
        }

        public override bool StartGrabbing()
        {
            lock (_syncLock)
            {
                try
                {
                    if (_device == null)
                        return false;

                    if (_status.IsGrabbing)
                        return true;

                    int ret = _device.StreamGrabber.StartGrabbing();
                    if (!IsSuccess(ret))
                        throw new Exception("StartGrabbing failed. ErrorCode = " + ret);

                    _grabThreadRunning = true;
                    _grabThread = new Thread(GrabLoop);
                    _grabThread.IsBackground = true;
                    _grabThread.Start();

                    _status.IsGrabbing = true;
                    _status.LastUpdateTime = DateTime.Now;

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision StartGrabbing failed.", ex);
                    return false;
                }
            }
        }

        public override bool StopGrabbing()
        {
            lock (_syncLock)
            {
                try
                {
                    StopGrabbingInternal();
                    _status.LastUpdateTime = DateTime.Now;
                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision StopGrabbing failed.", ex);
                    return false;
                }
            }
        }

        public override bool SoftTrigger()
        {
            lock (_syncLock)
            {
                try
                {
                    if (_device == null)
                        return false;

                    SetTriggerMode(TriggerMode.Software);

                    int ret = _device.Parameters.SetCommandValue("TriggerSoftware");
                    if (!IsSuccess(ret))
                        throw new Exception("TriggerSoftware failed. ErrorCode = " + ret);

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision SoftTrigger failed.", ex);
                    return false;
                }
            }
        }

        public override bool SetTriggerMode(TriggerMode mode)
        {
            lock (_syncLock)
            {
                try
                {
                    if (_config != null)
                        _config.TriggerMode = mode;

                    if (_device == null)
                        return true;

                    if (mode == TriggerMode.Continuous)
                    {
                        _device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
                        _device.Parameters.SetEnumValueByString("TriggerMode", "Off");
                    }
                    else if (mode == TriggerMode.Software)
                    {
                        _device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
                        _device.Parameters.SetEnumValueByString("TriggerMode", "On");
                        _device.Parameters.SetEnumValueByString("TriggerSource", "Software");
                    }
                    else if (mode == TriggerMode.Hardware)
                    {
                        _device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
                        _device.Parameters.SetEnumValueByString("TriggerMode", "On");
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision SetTriggerMode failed.", ex);
                    return false;
                }
            }
        }

        public override bool SetExposure(double value)
        {
            lock (_syncLock)
            {
                try
                {
                    if (_config != null)
                        _config.Exposure = value;

                    if (_device == null)
                        return true;

                    _device.Parameters.SetEnumValue("ExposureAuto", 0);
                    int ret = _device.Parameters.SetFloatValue("ExposureTime", (float)value);

                    if (!IsSuccess(ret))
                        throw new Exception("Set ExposureTime failed. ErrorCode = " + ret);

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision SetExposure failed.", ex);
                    return false;
                }
            }
        }

        public override bool SetGain(double value)
        {
            lock (_syncLock)
            {
                try
                {
                    if (_config != null)
                        _config.Gain = value;

                    if (_device == null)
                        return true;

                    _device.Parameters.SetEnumValue("GainAuto", 0);
                    int ret = _device.Parameters.SetFloatValue("Gain", (float)value);

                    if (!IsSuccess(ret))
                        throw new Exception("Set Gain failed. ErrorCode = " + ret);

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision SetGain failed.", ex);
                    return false;
                }
            }
        }

        public override bool SetFrameRate(double value)
        {
            lock (_syncLock)
            {
                try
                {
                    if (_config != null)
                        _config.FrameRate = value;

                    if (_device == null)
                        return true;

                    //try { _device.Parameters.SetBoolValue("AcquisitionFrameRateEnable", true); } catch { }
                    try { _device.Parameters.SetEnumValueByString("AcquisitionFrameRateEnable", "true"); } catch { }                    

                    int ret = _device.Parameters.SetFloatValue("AcquisitionFrameRate", (float)value);
                    if (!IsSuccess(ret))
                    {
                        // 部分機型不支援 FrameRate，不直接視為嚴重錯誤。
                        return true;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision SetFrameRate failed.", ex);
                    return false;
                }
            }
        }

        public override CameraInfo GetCameraInfo()
        {
            CameraInfo info = new CameraInfo();

            info.CameraId = CameraId;
            info.CameraName = CameraName;
            info.Brand = Brand;
            info.VendorName = "Hikvision";
            info.SerialNumber = _config != null ? _config.SerialNumber : string.Empty;
            info.IpAddress = _config != null ? _config.IpAddress : string.Empty;

            if (_deviceInfo != null)
            {
                info.ModelName = GetPropertyText(_deviceInfo, "ModelName");
                info.SerialNumber = GetPropertyText(_deviceInfo, "SerialNumber");
                info.IpAddress = GetPropertyText(_deviceInfo, "IpAddress");
            }

            return info;
        }

        public override void Dispose()
        {
            try { Close(); } catch { }
            base.Dispose();
        }

        private void ApplyOpenParameters()
        {
            if (_device == null || _config == null)
                return;

            SetTriggerMode(_config.TriggerMode);
            SetExposure(_config.Exposure);
            SetGain(_config.Gain);

            if (_config.FrameRate > 0)
                SetFrameRate(_config.FrameRate);
        }

        private IDeviceInfo FindDeviceInfo()
        {
            List<IDeviceInfo> infos = new List<IDeviceInfo>();

            DeviceEnumerator.EnumDevices(DeviceTLayerType.MvGigEDevice, out infos);

            if (infos == null || infos.Count == 0)
                return null;

            foreach (IDeviceInfo info in infos)
            {
                string sn = GetPropertyText(info, "SerialNumber");
                string userName = GetPropertyText(info, "UserDefinedName");

                if (!string.IsNullOrWhiteSpace(_config.SerialNumber) && sn == _config.SerialNumber)
                    return info;

                if (!string.IsNullOrWhiteSpace(_config.CameraName) && userName == _config.CameraName)
                    return info;
            }

            return infos[0];
        }

        private void GrabLoop()
        {
            while (_grabThreadRunning)
            {
                IFrameOut frameOut = null;

                try
                {
                    int ret = _device.StreamGrabber.GetImageBuffer(1000, out frameOut);

                    if (IsSuccess(ret) && frameOut != null)
                    {
                        CameraFrame frame = ConvertToCameraFrame(frameOut);
                        RaiseImageGrabbed(frame);
                    }
                }
                catch (Exception ex)
                {
                    RaiseError("Hikvision GrabLoop failed.", ex);
                    Thread.Sleep(20);
                }
                finally
                {
                    if (frameOut != null && _device != null)
                    {
                        try { _device.StreamGrabber.FreeImageBuffer(frameOut); } catch { }
                    }
                }
            }

            _status.IsGrabbing = false;
            _status.LastUpdateTime = DateTime.Now;
        }

        private void StopGrabbingInternal()
        {
            _grabThreadRunning = false;

            if (_grabThread != null)
            {
                if (!_grabThread.Join(300))
                {
                    try { _grabThread.Interrupt(); } catch { }
                }

                _grabThread = null;
            }

            if (_device != null)
            {
                try { _device.StreamGrabber.StopGrabbing(); } catch { }
            }

            _status.IsGrabbing = false;
        }

        private CameraFrame ConvertToCameraFrame(IFrameOut frameOut)
        {
            using (Bitmap bmp = frameOut.Image.ToBitmap())
            {
                return ConvertBitmapToCameraFrame(bmp);
            }
        }

        private CameraFrame ConvertBitmapToCameraFrame(Bitmap bitmap)
        {
            Bitmap source = null;
            BitmapData bmpData = null;

            try
            {
                source = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);

                using (Graphics g = Graphics.FromImage(source))
                {
                    g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
                }

                Rectangle rect = new Rectangle(0, 0, source.Width, source.Height);
                bmpData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                int stride = Math.Abs(bmpData.Stride);
                int bufferSize = stride * source.Height;
                byte[] buffer = new byte[bufferSize];

                Marshal.Copy(bmpData.Scan0, buffer, 0, bufferSize);

                CameraFrame frame = new CameraFrame();
                frame.CameraId = CameraId;
                frame.Timestamp = DateTime.Now;
                frame.Width = source.Width;
                frame.Height = source.Height;
                frame.Buffer = buffer;
                frame.PixelFormat = "RGB24";
                frame.FrameNumber = Interlocked.Increment(ref _frameNumber);

                return frame;
            }
            finally
            {
                if (bmpData != null && source != null)
                    source.UnlockBits(bmpData);

                if (source != null)
                    source.Dispose();
            }
        }

        private bool IsSuccess(int ret)
        {
            return ret == MvError.MV_OK || ret == MvError.MV_ALG_OK;
        }

        private string GetPropertyText(object obj, string propertyName)
        {
            try
            {
                if (obj == null)
                    return string.Empty;

                PropertyInfo prop = obj.GetType().GetProperty(propertyName);
                if (prop == null)
                    return string.Empty;

                object value = prop.GetValue(obj, null);
                return value != null ? value.ToString() : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
