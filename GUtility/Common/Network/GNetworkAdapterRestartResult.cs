using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Network
{
    /// <summary>
    /// 網路介面卡重啟結果。
    /// </summary>
    public class GNetworkAdapterRestartResult
    {
        public bool Success { get; private set; }

        public string Message { get; private set; }

        public static GNetworkAdapterRestartResult Ok()
        {
            return new GNetworkAdapterRestartResult
            {
                Success = true,
                Message = "Network adapter restarted successfully."
            };
        }

        public static GNetworkAdapterRestartResult Fail(string message)
        {
            return new GNetworkAdapterRestartResult
            {
                Success = false,
                Message = message
            };
        }
    }
}