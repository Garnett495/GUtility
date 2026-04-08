using Basler.Pylon;
using GUtility.Module.GCamera.Core;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace GUtility.Module.GCamera.Providers.Basler
{
    /// <summary>
    /// Basler 相機實作類別
    /// 
    /// 功能說明：
    /// 1. 依 SerialNumber 開啟指定 Basler 相機
    /// 2. 支援連續取像與單張取像
    /// 3. 支援軟體觸發模式
    /// 4. 支援曝光、增益、幀率設定
    /// 5. 將 Basler SDK 的影像結果轉為系統內的 CameraFrame
    /// 
    /// 注意：
    /// - 本類別依賴 Basler.Pylon SDK
    /// - 本類別僅處理相機控制，不負責 UI 顯示
    /// </summary>
    public class BaslerCamera : CameraBase
    {

        #region ===== Properties =====

        //-------- Private Fields --------
        private readonly object _syncLock = new object();

        private Camera _camera;
        private PixelDataConverter _converter;
        private Thread _grabThread;

        private volatile bool _grabThreadRunning;
        private long _frameNumber;

        private const int RGB_CHANNEL_COUNT = 3;

        //-------- Public Properties --------
        public override CameraBrand Brand
        {
            get { return CameraBrand.Basler; }
        }

        #endregion

        //-------- Constructor --------

        public BaslerCamera() : base()
        {
            _converter = new PixelDataConverter();
            _camera = null;
            _grabThread = null;
            _grabThreadRunning = false;
            _frameNumber = 0;
        }


        //-------- Public Methods --------

        /// <summary>
        /// 搜尋目前可用的 Basler 相機序號
        /// </summary>
        public static List<string> SearchAvailableCamera()
        {
            List<string> serials = new List<string>();
            List<ICameraInfo> devices = new List<ICameraInfo>(CameraFinder.Enumerate());

            foreach (ICameraInfo device in devices)
            {
                if (device.ContainsKey(CameraInfoKey.SerialNumber))
                {
                    serials.Add(device[CameraInfoKey.SerialNumber]);
                }
            }

            return serials;
        }

        public override bool Initialize(CameraConfig config)
        {
            if (!base.Initialize(config))
                return false;

            if (_config.Brand != CameraBrand.Basler)
                _config.Brand = CameraBrand.Basler;

            return true;
        }

        public override bool Open()
        {
            lock (_syncLock)
            {
                if (!IsInitialized)
                    return false;

                if (_camera != null && _camera.IsOpen)
                {
                    _status.IsOpen = true;
                    _status.LastUpdateTime = DateTime.Now;
                    return true;
                }

                try
                {
                    string serialNumber;
                    List<ICameraInfo> devices;
                    ICameraInfo cameraInfo;

                    serialNumber = _config != null ? _config.SerialNumber : null;
                    if (string.IsNullOrWhiteSpace(serialNumber))
                        throw new Exception("Basler SerialNumber is empty.");

                    devices = CameraFinder.Enumerate();
                    cameraInfo = devices.Find(d => d[CameraInfoKey.SerialNumber] == serialNumber);

                    if (cameraInfo == null)
                        throw new Exception("Basler camera not found. SerialNumber = " + serialNumber);

                    if (_camera != null)
                    {
                        SafeCloseAndDisposeCamera();
                    }

                    _camera = new Camera(cameraInfo);
                    _camera.Open(200, TimeoutHandling.Return);

                    ApplyOpenParameters();

                    _status.IsInitialized = true;
                    _status.IsOpen = _camera.IsOpen;
                    _status.IsGrabbing = false;
                    _status.LastErrorMessage = string.Empty;
                    _status.LastUpdateTime = DateTime.Now;

                    return _status.IsOpen;
                }
                catch (Exception ex)
                {
                    RaiseError("Basler Open failed.", ex);
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

                    SafeCloseAndDisposeCamera();

                    _status.IsOpen = false;
                    _status.IsGrabbing = false;
                    _status.LastUpdateTime = DateTime.Now;

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Basler Close failed.", ex);
                    return false;
                }
            }
        }

        public override bool StartGrabbing()
        {
            lock (_syncLock)
            {
                if (_camera == null)
                    return false;

                if (!_camera.IsOpen)
                {
                    if (!Open())
                        return false;
                }

                if (_status.IsGrabbing)
                    return true;

                try
                {
                    _camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);

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
                    RaiseError("Basler StartGrabbing failed.", ex);
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
                    RaiseError("Basler StopGrabbing failed.", ex);
                    return false;
                }
            }
        }

        public override bool SoftTrigger()
        {
            lock (_syncLock)
            {
                if (_camera == null || !_camera.IsOpen)
                    return false;

                try
                {
                    SetTriggerMode(TriggerMode.Software);

                    IParameter parameter = _camera.Parameters[PLCamera.TriggerSoftware];
                    ICommandParameter commandParameter = parameter as ICommandParameter;
                    if (commandParameter == null)
                        throw new Exception("TriggerSoftware command parameter is not available.");

                    commandParameter.Execute();
                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Basler SoftTrigger failed.", ex);
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

                    if (_camera == null || !_camera.IsOpen)
                        return true;

                    switch (mode)
                    {
                        case TriggerMode.Continuous:
                            _camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
                            _camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                            break;

                        case TriggerMode.Software:
                            _camera.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
                            _camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            _camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Software);
                            _camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                            break;

                        case TriggerMode.Hardware:
                            _camera.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
                            _camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                            _camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                            break;

                        default:
                            _camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.Off);
                            break;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Basler SetTriggerMode failed.", ex);
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

                    if (_camera == null || !_camera.IsConnected)
                        return true;

                    if (_camera.Parameters[PLCamera.ExposureTimeAbs].IsWritable)
                    {
                        _camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(value);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Basler SetExposure failed.", ex);
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

                    if (_camera == null || !_camera.IsConnected)
                        return true;

                    if (_camera.Parameters[PLCamera.GainRaw].IsWritable)
                    {
                        _camera.Parameters[PLCamera.GainRaw].SetValue((int)value);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Basler SetGain failed.", ex);
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

                    if (_camera == null || !_camera.IsConnected)
                        return true;

                    // 注意：
                    // 不同 Basler 型號對 FrameRate 節點支援不同。
                    // 這裡先保留 config，同時嘗試常見節點名稱。
                    if (_camera.Parameters.Contains(PLCamera.AcquisitionFrameRateAbs))
                    {
                        if (_camera.Parameters[PLCamera.AcquisitionFrameRateAbs].IsWritable)
                            _camera.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(value);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    RaiseError("Basler SetFrameRate failed.", ex);
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
            info.VendorName = "Basler";
            info.SerialNumber = _config != null ? _config.SerialNumber : string.Empty;
            info.IpAddress = _config != null ? _config.IpAddress : string.Empty;

            try
            {
                if (_camera != null)
                {
                    if (_camera.CameraInfo.ContainsKey(CameraInfoKey.ModelName))
                        info.ModelName = _camera.CameraInfo[CameraInfoKey.ModelName];

                    if (_camera.CameraInfo.ContainsKey(CameraInfoKey.SerialNumber))
                        info.SerialNumber = _camera.CameraInfo[CameraInfoKey.SerialNumber];

                    if (_camera.CameraInfo.ContainsKey(CameraInfoKey.DeviceIpAddress))
                        info.IpAddress = _camera.CameraInfo[CameraInfoKey.DeviceIpAddress];

                    if (_camera.Parameters.Contains(PLCamera.WidthMax))
                        info.MaxWidth = (int)_camera.Parameters[PLCamera.WidthMax].GetValue();

                    if (_camera.Parameters.Contains(PLCamera.HeightMax))
                        info.MaxHeight = (int)_camera.Parameters[PLCamera.HeightMax].GetValue();

                    if (_camera.Parameters.Contains(PLCamera.PixelFormat))
                        info.PixelFormat = _camera.Parameters[PLCamera.PixelFormat].GetValue().ToString();
                }
            }
            catch
            {
                // 讀資訊失敗不拋例外，避免 UI 查詢資訊時中斷
            }

            return info;
        }

        public override void Dispose()
        {
            try
            {
                Close();
            }
            catch
            {
            }

            base.Dispose();
        }


        #region ===== Basler Methods =====

        /// 單張取像
        /// </summary>
        public Bitmap GrabOne()
        {
            lock (_syncLock)
            {
                if (_camera == null || !_camera.IsOpen)
                    return null;

                try
                {
                    _camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);

                    using (IGrabResult result = _camera.StreamGrabber.GrabOne(1000, TimeoutHandling.ThrowException))
                    {
                        if (result != null && result.GrabSucceeded)
                        {
                            return ConvertToBitmap(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RaiseError("Basler GrabOne failed.", ex);
                }

                return null;
            }
        }

        private void ApplyOpenParameters()
        {
            if (_camera == null || !_camera.IsOpen)
                return;

            _camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);

            if (_config != null)
            {
                SetTriggerMode(_config.TriggerMode);
                SetExposure(_config.Exposure);
                SetGain(_config.Gain);
                SetFrameRate(_config.FrameRate);
            }
        }

        #endregion

        #region ===== Private Methods =====

        private void GrabLoop()
        {
            try
            {
                _camera.StreamGrabber.Start();

                while (_grabThreadRunning)
                {
                    try
                    {
                        using (IGrabResult result = _camera.StreamGrabber.RetrieveResult(1000, TimeoutHandling.Return))
                        {
                            if (result == null)
                            {
                                Thread.Sleep(5);
                                continue;
                            }

                            if (!result.GrabSucceeded)
                            {
                                Thread.Sleep(5);
                                continue;
                            }

                            CameraFrame frame = ConvertToCameraFrame(result);
                            RaiseImageGrabbed(frame);
                        }
                    }
                    catch (Exception ex)
                    {
                        RaiseError("Basler GrabLoop failed.", ex);
                        Thread.Sleep(20);
                    }
                }
            }
            finally
            {
                try
                {
                    if (_camera != null && _camera.StreamGrabber.IsGrabbing)
                        _camera.StreamGrabber.Stop();
                }
                catch
                {
                }

                _status.IsGrabbing = false;
                _status.LastUpdateTime = DateTime.Now;
            }
        }

        private void StopGrabbingInternal()
        {
            _grabThreadRunning = false;

            if (_grabThread != null)
            {
                if (!_grabThread.Join(300))
                {
                    try
                    {
                        _grabThread.Interrupt();
                    }
                    catch
                    {
                    }
                }

                _grabThread = null;
            }

            if (_camera != null)
            {
                try
                {
                    if (_camera.StreamGrabber.IsGrabbing)
                        _camera.StreamGrabber.Stop();
                }
                catch
                {
                }
            }

            _status.IsGrabbing = false;
        }

        private void SafeCloseAndDisposeCamera()
        {
            if (_camera == null)
                return;

            try
            {
                if (_camera.StreamGrabber.IsGrabbing)
                    _camera.StreamGrabber.Stop();
            }
            catch
            {
            }

            try
            {
                if (_camera.IsOpen)
                    _camera.Close();
            }
            catch
            {
            }

            try
            {
                _camera.Dispose();
            }
            catch
            {
            }

            _camera = null;
        }

        private CameraFrame ConvertToCameraFrame(IGrabResult result)
        {
            byte[] buffer;
            CameraFrame frame;
            GCHandle handle = default(GCHandle);

            try
            {
                buffer = new byte[result.Width * result.Height * RGB_CHANNEL_COUNT];

                // 固定 byte[]，取得記憶體位址
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                _converter.OutputPixelFormat = PixelType.RGB8packed;
                _converter.Convert(handle.AddrOfPinnedObject(), buffer.Length, result);

                frame = new CameraFrame();
                frame.CameraId = CameraId;
                frame.Timestamp = DateTime.Now;
                frame.Width = result.Width;
                frame.Height = result.Height;
                frame.Buffer = buffer;
                frame.PixelFormat = "RGB8packed";
                frame.FrameNumber = Interlocked.Increment(ref _frameNumber);

                return frame;
            }
            finally
            {
                if (handle.IsAllocated)
                    handle.Free();
            }
        }

        private Bitmap ConvertToBitmap(IGrabResult result)
        {
            Bitmap bitmap;
            BitmapData bmpData;

            bitmap = new Bitmap(result.Width, result.Height, PixelFormat.Format24bppRgb);
            bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            _converter.OutputPixelFormat = PixelType.RGB8packed;
            _converter.Convert(bmpData.Scan0, bmpData.Stride * bitmap.Height, result);

            bitmap.UnlockBits(bmpData);
            return bitmap;
        }


        #endregion
    }
}
