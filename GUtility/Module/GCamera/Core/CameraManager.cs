using GUtility.Module.GCamera.Abstraction;
using GUtility.Module.GCamera.Core;
using GUtility.Module.GCamera.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Module.GCamera.Core
{
    /// <summary>
    /// 多台相機管理器
    /// </summary>
    public class CameraManager : IDisposable
    {
        private readonly Dictionary<string, ICamera> _cameraMap;
        private readonly ICameraFactory _factory;

        public CameraManager()
        {
            _cameraMap = new Dictionary<string, ICamera>();
            _factory = new CameraFactory();
        }

        /// <summary>
        /// 新增相機
        /// </summary>
        public bool AddCamera(CameraConfig config)
        {
            ICamera camera;

            if (config == null)
                return false;

            if (string.IsNullOrWhiteSpace(config.CameraId))
                return false;

            if (_cameraMap.ContainsKey(config.CameraId))
                return false;

            camera = _factory.CreateCamera(config.Brand);
            if (!camera.Initialize(config))
                return false;

            _cameraMap.Add(config.CameraId, camera);
            return true;
        }

        /// <summary>
        /// 取得指定相機
        /// </summary>
        public ICamera GetCamera(string cameraId)
        {
            if (string.IsNullOrWhiteSpace(cameraId))
                return null;

            if (_cameraMap.ContainsKey(cameraId))
                return _cameraMap[cameraId];

            return null;
        }

        /// <summary>
        /// 開啟全部相機
        /// </summary>
        public bool OpenAll()
        {
            bool result = true;

            foreach (ICamera camera in _cameraMap.Values)
            {
                if (!camera.Open())
                    result = false;
            }

            return result;
        }

        /// <summary>
        /// 關閉全部相機
        /// </summary>
        public bool CloseAll()
        {
            bool result = true;

            foreach (ICamera camera in _cameraMap.Values)
            {
                if (!camera.Close())
                    result = false;
            }

            return result;
        }

        public void Dispose()
        {
            foreach (ICamera camera in _cameraMap.Values)
            {
                camera.Dispose();
            }

            _cameraMap.Clear();
        }
    }
}
