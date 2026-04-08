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

        /// <summary>
        /// 執行緒同步鎖，避免多執行緒同時操作相機資源。
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// Basler SDK 相機物件。
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// Basler SDK 像素格式轉換器。
        /// </summary>
        private PixelDataConverter _converter;

        /// <summary>
        /// 背景抓圖執行緒。
        /// </summary>
        private Thread _grabThread;

        /// <summary>
        /// 控制抓圖執行緒是否持續執行。
        /// </summary>
        private volatile bool _grabThreadRunning;

        /// <summary>
        /// 影像流水號計數器。
        /// </summary>
        private long _frameNumber;

        /// <summary>
        /// RGB 圖像通道數。
        /// RGB8packed = 3 channels。
        /// </summary>
        private const int RGB_CHANNEL_COUNT = 3;

        //-------- Public Properties --------

        /// <summary>
        /// 目前相機品牌。
        /// </summary>
        public override CameraBrand Brand
        {
            get { return CameraBrand.Basler; }
        }

        #endregion

        //-------- Constructor --------

        /// <summary>
        /// 建立 BaslerCamera 實例。
        /// 初始化轉換器與內部狀態。
        /// </summary>
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
        /// 搜尋目前電腦可偵測到的 Basler 相機序號清單。
        /// 
        /// 用途：
        /// - 系統啟動時列出可用相機
        /// - 給 UI 或設定流程做設備選擇
        /// </summary>
        /// <returns>Basler 相機序號列表</returns>
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

        /// <summary>
        /// 初始化相機設定。
        /// 
        /// 功能：
        /// - 呼叫基底類別初始化流程
        /// - 強制將 Brand 設定為 Basler
        /// </summary>
        /// <param name="config">相機設定物件</param>
        /// <returns>初始化成功回傳 true，否則 false</returns>
        public override bool Initialize(CameraConfig config)
        {
            if (!base.Initialize(config))
                return false;

            if (_config.Brand != CameraBrand.Basler)
                _config.Brand = CameraBrand.Basler;

            return true;
        }

        /// <summary>
        /// 開啟指定 Basler 相機。
        /// 
        /// 流程：
        /// 1. 依 _config.SerialNumber 搜尋相機
        /// 2. 建立 Basler Camera 物件
        /// 3. 呼叫 SDK 開啟相機
        /// 4. 套用初始化參數（Trigger / Exposure / Gain / FrameRate）
        /// </summary>
        /// <returns>開啟成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 關閉相機並釋放資源。
        /// 
        /// 流程：
        /// 1. 停止抓圖執行緒
        /// 2. 停止 StreamGrabber
        /// 3. 關閉並釋放 Basler Camera
        /// </summary>
        /// <returns>關閉成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 開始連續取像。
        /// 
        /// 功能：
        /// - 確保相機已開啟
        /// - 將 AcquisitionMode 設為 Continuous
        /// - 啟動背景抓圖執行緒 GrabLoop
        /// </summary>
        /// <returns>啟動成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 停止連續取像。
        /// 
        /// 功能：
        /// - 停止背景抓圖執行緒
        /// - 停止 StreamGrabber
        /// - 更新狀態為未抓圖
        /// </summary>
        /// <returns>停止成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 執行一次軟體觸發。
        /// 
        /// 前提：
        /// - 相機已開啟
        /// - TriggerMode 需切到 Software
        /// </summary>
        /// <returns>觸發成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 設定觸發模式。
        /// 
        /// 支援模式：
        /// - Continuous：連續取像，不啟用 Trigger
        /// - Software：軟體觸發
        /// - Hardware：硬體觸發（目前僅開啟 Trigger，實際輸入線需依機型再補）
        /// </summary>
        /// <param name="mode">觸發模式</param>
        /// <returns>設定成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 設定曝光時間。
        /// 
        /// 功能：
        /// - 同步更新 _config.Exposure
        /// - 若相機已連線，則立即寫入 Basler 參數節點
        /// </summary>
        /// <param name="value">曝光時間</param>
        /// <returns>設定成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 設定增益。
        /// 
        /// 功能：
        /// - 同步更新 _config.Gain
        /// - 若相機已連線，則立即寫入 Basler 參數節點
        /// </summary>
        /// <param name="value">增益值</param>
        /// <returns>設定成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 設定幀率。
        /// 
        /// 注意：
        /// - 不同 Basler 型號的 FrameRate 節點支援度不同
        /// - 若節點存在且可寫，才會實際下參數
        /// </summary>
        /// <param name="value">幀率值</param>
        /// <returns>設定成功回傳 true，否則 false</returns>
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

        /// <summary>
        /// 取得相機資訊。
        /// 
        /// 內容包含：
        /// - CameraId / CameraName / Brand
        /// - Vendor / Model / SerialNumber / IpAddress
        /// - MaxWidth / MaxHeight / PixelFormat
        /// </summary>
        /// <returns>相機資訊物件</returns>
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

        /// <summary>
        /// 釋放相機資源。
        /// 
        /// 流程：
        /// - 先執行 Close()
        /// - 再呼叫基底類別 Dispose()
        /// </summary>
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

        /// <summary>
        /// 單張取像。
        /// 
        /// 功能：
        /// - 將 AcquisitionMode 設為 SingleFrame
        /// - 透過 GrabOne 擷取一張影像
        /// - 將結果轉為 Bitmap 回傳
        /// 
        /// 適用情境：
        /// - 測試畫面
        /// - 手動拍照
        /// - 工程模式單次取像
        /// </summary>
        /// <returns>成功回傳 Bitmap，失敗回傳 null</returns>
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

        /// <summary>
        /// 套用相機開啟後的初始化參數。
        /// 
        /// 內容包含：
        /// - AcquisitionMode = Continuous
        /// - TriggerMode
        /// - Exposure
        /// - Gain
        /// - FrameRate
        /// </summary>
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

        /// <summary>
        /// 背景連續抓圖迴圈。
        /// 
        /// 流程：
        /// 1. 啟動 StreamGrabber
        /// 2. 持續 RetrieveResult
        /// 3. 將每張影像轉為 CameraFrame
        /// 4. 透過 RaiseImageGrabbed 發送事件
        /// 
        /// 注意：
        /// - 執行於背景執行緒
        /// - 若擷取失敗，會短暫休眠後繼續
        /// </summary>
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

        /// <summary>
        /// 內部停止抓圖流程。
        /// 
        /// 功能：
        /// - 停止抓圖執行緒旗標
        /// - 等待執行緒結束
        /// - 嘗試中止 StreamGrabber
        /// 
        /// 注意：
        /// - 此方法不關閉相機
        /// - 只負責停止抓圖
        /// </summary>
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


        /// <summary>
        /// 安全地關閉並釋放 Basler Camera 物件。
        /// 
        /// 功能：
        /// - 停止 StreamGrabber
        /// - 關閉相機
        /// - Dispose SDK 資源
        /// - 清除 _camera 參考
        /// </summary>
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

        /// <summary>
        /// 將 Basler IGrabResult 轉為系統內部的 CameraFrame。
        /// 
        /// 流程：
        /// 1. 建立 RGB byte[] 緩衝區
        /// 2. 使用 GCHandle 固定記憶體位址
        /// 3. 透過 PixelDataConverter 轉為 RGB8packed
        /// 4. 建立 CameraFrame 並填入影像資料
        /// 
        /// 注意：
        /// - 本方法使用 pinned memory，結束後一定要 Free()
        /// </summary>
        /// <param name="result">Basler 抓圖結果</param>
        /// <returns>轉換後的 CameraFrame</returns>
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

        /// <summary>
        /// 將 Basler IGrabResult 轉為 Bitmap。
        /// 
        /// 適用：
        /// - 單張取像
        /// - UI 測試顯示
        /// 
        /// 注意：
        /// - 目前輸出為 24bpp RGB
        /// </summary>
        /// <param name="result">Basler 抓圖結果</param>
        /// <returns>Bitmap 影像</returns>
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
