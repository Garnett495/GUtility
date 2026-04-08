using GUtility.Module.GCamera.Abstraction;
using GUtility.Module.GCamera.Core;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Module.GCamera.Example.MultiCamera
{
    /// <summary>
    /// 多相機使用範例
    /// </summary>
    public class MultiCameraExample
    {
        public void Run()
        {
            CameraManager manager = new CameraManager();

            // CAM1
            manager.AddCamera(new CameraConfig()
            {
                CameraId = "CAM1",
                Brand = CameraBrand.Basler
            });

            // CAM2
            manager.AddCamera(new CameraConfig()
            {
                CameraId = "CAM2",
                Brand = CameraBrand.Hikvision
            });

            manager.OpenAll();

            ICamera cam1 = manager.GetCamera("CAM1");
            ICamera cam2 = manager.GetCamera("CAM2");

            cam1.StartGrabbing();
            cam2.StartGrabbing();

            Console.WriteLine("Running... Press any key");
            Console.ReadKey();

            manager.CloseAll();
            manager.Dispose();
        }
    }
}
