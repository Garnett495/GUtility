using GUtility.Module.GCamera.Abstraction;
using GUtility.Module.GCamera.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.GCamera.Abstraction
{
    /// <summary>
    /// 相機建立工廠介面
    /// </summary>
    public interface ICameraFactory
    {
        ICamera CreateCamera(CameraBrand brand);
    }
}
