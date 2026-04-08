using GUtility.Module.GCamera.Abstraction;
using GUtility.Module.GCamera.Core;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Module.GCamera.Example.BasicUsage
{
    /// <summary>
    /// 最基本相機使用範例
    /// </summary>
    public class CameraBasicExample
    {
        public void Run()
        {
            CameraManager manager = new CameraManager();

            // 1. 建立設定
            CameraConfig config = new CameraConfig();
            config.CameraId = "CAM1";
            config.CameraName = "Top Camera";
            config.Brand = CameraBrand.Basler;
            config.SerialNumber = "12345678";
            config.TriggerMode = TriggerMode.Software;
            config.Exposure = 5000;

            // 2. 加入相機
            if (!manager.AddCamera(config))
            {
                Console.WriteLine("AddCamera failed.");
                return;
            }

            // 3. 取得相機
            ICamera camera = manager.GetCamera("CAM1");
            if (camera == null)
            {
                Console.WriteLine("GetCamera failed.");
                return;
            }

            // 4. 註冊事件
            camera.ImageGrabbed += OnImageGrabbed;
            camera.ErrorOccurred += OnError;

            // 5. 開啟相機
            if (!camera.Open())
            {
                Console.WriteLine("Open failed.");
                return;
            }

            // 6. 設定參數
            camera.SetExposure(8000);
            camera.SetGain(1.0);

            // 7. 開始取像
            if (!camera.StartGrabbing())
            {
                Console.WriteLine("StartGrabbing failed.");
                return;
            }

            // 8. 軟觸發
            camera.SoftTrigger();

            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // 9. 停止與關閉
            camera.StopGrabbing();
            camera.Close();

            manager.Dispose();
        }

        /// <summary>
        /// 影像回呼
        /// </summary>
        private void OnImageGrabbed(object sender, Events.CameraImageEventArgs e)
        {
            if (e.Frame == null)
                return;

            Console.WriteLine("Image received: " + e.Frame.Width + "x" + e.Frame.Height);
        }

        /// <summary>
        /// 錯誤回呼
        /// </summary>
        private void OnError(object sender, Events.CameraErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}