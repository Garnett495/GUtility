using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.GCamera.Abstraction;
using GUtility.Module.GCamera.Enums;
using GUtility.Module.GCamera.Providers.Basler;
using GUtility.Module.GCamera.Providers.Dalsa;
using GUtility.Module.GCamera.Providers.Hikvision;


namespace GUtility.Module.GCamera.Core
{
    /// <summary>
    /// 相機工廠
    /// </summary>
    public class CameraFactory : ICameraFactory
    {
        public ICamera CreateCamera(CameraBrand brand)
        {
            switch (brand)
            {
                case CameraBrand.Basler:
                    return new BaslerCamera();

                case CameraBrand.Dalsa:
                    return new DalsaCamera();

                case CameraBrand.Hikvision:
                    return new HikCamera();

                default:
                    throw new NotSupportedException("Unsupported camera brand: " + brand);
            }
        }
    }
}
