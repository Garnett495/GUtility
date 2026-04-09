using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Module.IO.Abstractions;

namespace GUtility.Module.IO.Management
{
    /// <summary>
    /// 統一管理多個 I/O 裝置。
    /// </summary>
    public class GIoManager : IDisposable
    {
        private readonly Dictionary<string, IGDevice> _devices;
        private bool _disposed;

        public GIoManager()
        {
            _devices = new Dictionary<string, IGDevice>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 註冊裝置。
        /// </summary>
        public void AddDevice(IGDevice device)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (string.IsNullOrWhiteSpace(device.DeviceName))
                throw new ArgumentException("DeviceName cannot be empty.");

            if (_devices.ContainsKey(device.DeviceName))
                throw new InvalidOperationException("Device already exists: " + device.DeviceName);

            _devices.Add(device.DeviceName, device);
        }

        /// <summary>
        /// 移除裝置。
        /// </summary>
        public bool RemoveDevice(string deviceName)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                return false;

            IGDevice device;
            if (_devices.TryGetValue(deviceName, out device))
            {
                device.Dispose();
                return _devices.Remove(deviceName);
            }

            return false;
        }

        /// <summary>
        /// 取得指定裝置。
        /// </summary>
        public IGDevice GetDevice(string deviceName)
        {
            IGDevice device;
            _devices.TryGetValue(deviceName, out device);
            return device;
        }

        /// <summary>
        /// 取得指定型別裝置。
        /// </summary>
        public T GetDevice<T>(string deviceName) where T : class, IGDevice
        {
            return GetDevice(deviceName) as T;
        }

        /// <summary>
        /// 連線所有裝置。
        /// </summary>
        public void ConnectAll()
        {
            foreach (KeyValuePair<string, IGDevice> item in _devices)
            {
                item.Value.Connect();
            }
        }

        /// <summary>
        /// 中斷所有裝置。
        /// </summary>
        public void DisconnectAll()
        {
            foreach (KeyValuePair<string, IGDevice> item in _devices)
            {
                item.Value.Disconnect();
            }
        }

        /// <summary>
        /// 更新所有裝置快取。
        /// </summary>
        public void RefreshAll()
        {
            foreach (KeyValuePair<string, IGDevice> item in _devices)
            {
                item.Value.Refresh();
            }
        }

        /// <summary>
        /// 取得所有裝置列表。
        /// </summary>
        public IList<IGDevice> GetAllDevices()
        {
            List<IGDevice> result = new List<IGDevice>();
            foreach (KeyValuePair<string, IGDevice> item in _devices)
            {
                result.Add(item.Value);
            }
            return result;
        }

        /// <summary>
        /// 釋放裝置資源。
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            foreach (KeyValuePair<string, IGDevice> item in _devices)
            {
                item.Value.Dispose();
            }

            _devices.Clear();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
