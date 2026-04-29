using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.GCamera.Core;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Models;

namespace GUtility.Module.GCamera.Example.HikvisionBasicUsage
{
    public class HikCameraBasicExample
    {
        public void Run()
        {
            CameraManager manager = new CameraManager();

            CameraConfig config = new CameraConfig();
            config.CameraId = "CAM1";
            config.CameraName = "Hik Camera";
            config.Brand = CameraBrand.Hikvision;
            config.SerialNumber = "YOUR_SN";
            config.Exposure = 10000;
            config.Gain = 1;
            config.TriggerMode = TriggerMode.Continuous;

            manager.AddCamera(config);

            var cam = manager.GetCamera("CAM1");
            if (cam == null)
                return;

            cam.ImageGrabbed += (sender, e) =>
            {
                if (e.Frame != null)
                    Console.WriteLine("Frame: " + e.Frame.FrameNumber);
            };

            cam.ErrorOccurred += (sender, e) =>
            {
                Console.WriteLine("Error: " + e.Message);
            };

            cam.Open();
            cam.StartGrabbing();

            Console.ReadKey();

            cam.StopGrabbing();
            cam.Close();
            manager.Dispose();
        }
    }
}
