using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Network
{
    /// <summary>
    /// 網路介面卡重啟設定。
    /// </summary>
    public class GNetworkAdapterRestartOptions
    {
        /// <summary>
        /// Windows 網路介面名稱，例如：乙太網路、乙太網路 2、Ethernet。
        /// </summary>
        public string AdapterName { get; set; }

        /// <summary>
        /// 停用網卡後等待時間。
        /// </summary>
        public int DisableWaitMs { get; set; }

        /// <summary>
        /// 啟用網卡後等待網路恢復時間。
        /// </summary>
        public int EnableWaitMs { get; set; }

        public GNetworkAdapterRestartOptions()
        {
            DisableWaitMs = 5000;
            EnableWaitMs = 5000;
        }
    }
}