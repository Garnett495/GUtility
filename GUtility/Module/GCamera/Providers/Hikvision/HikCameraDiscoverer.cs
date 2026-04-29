using GUtility.Module.GCamera.Models;
using MvCamCtrl.NET;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MvCameraControl;

using GUtility.Module.GCamera.Enums;

namespace GUtility.Module.GCamera.Providers.Hikvision
{
    /// <summary>
    /// Hikvision / Hikrobot 相機搜尋工具。
    /// </summary>
    public class HikCameraDiscoverer
    {
        public List<CameraInfo> Search()
        {
            List<CameraInfo> result = new List<CameraInfo>();
            List<IDeviceInfo> infos = new List<IDeviceInfo>();

            DeviceEnumerator.EnumDevices(DeviceTLayerType.MvGigEDevice, out infos);

            if (infos == null)
                return result;

            foreach (IDeviceInfo deviceInfo in infos)
            {
                CameraInfo info = new CameraInfo();

                info.Brand = CameraBrand.Hikvision;
                info.VendorName = "Hikvision";
                info.CameraName = GetPropertyText(deviceInfo, "UserDefinedName");
                info.ModelName = GetPropertyText(deviceInfo, "ModelName");
                info.SerialNumber = GetPropertyText(deviceInfo, "SerialNumber");
                info.IpAddress = GetPropertyText(deviceInfo, "IpAddress");

                result.Add(info);
            }

            return result;
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
