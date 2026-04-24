using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Network;

namespace GUtility.Common.Network.Example
{
    /// <summary>
    /// 網路介面卡重啟範例。
    /// 
    /// 使用情境：
    /// 1. GigE 相機斷線
    /// 2. 網卡被占用無法重新連線
    /// 3. AOI 設備自動復歸流程
    /// </summary>
    public class NetworkAdapterRestartExample
    {
        /// <summary>
        /// 基本使用範例。
        /// </summary>
        public static void RunBasicExample()
        {
            var restarter = new GNetworkAdapterRestarter();

            var options = new GNetworkAdapterRestartOptions
            {
                AdapterName = "乙太網路 2", // ⚠️ 請改成實際網卡名稱
                DisableWaitMs = 3000,
                EnableWaitMs = 5000
            };

            var result = restarter.Restart(options);

            if (result.Success)
            {
                Console.WriteLine("Network restart success.");
            }
            else
            {
                Console.WriteLine("Network restart failed: " + result.Message);
            }
        }

        /// <summary>
        /// AOI 常見情境：相機斷線後自動恢復。
        /// </summary>
        public static void RunCameraRecoveryLikeScenario()
        {
            var restarter = new GNetworkAdapterRestarter();

            var options = new GNetworkAdapterRestartOptions
            {
                AdapterName = "乙太網路 2",
                DisableWaitMs = 5000,
                EnableWaitMs = 5000
            };

            Console.WriteLine("Simulate camera disconnected...");

            // Step 1：模擬釋放相機資源
            Console.WriteLine("Stop grab...");
            Console.WriteLine("Close camera...");

            // Step 2：重啟網卡
            var result = restarter.Restart(options);

            if (!result.Success)
            {
                Console.WriteLine("Recovery failed: " + result.Message);
                return;
            }

            // Step 3：模擬重新連線
            Console.WriteLine("Reconnect camera...");
            Console.WriteLine("Start grab...");

            Console.WriteLine("Recovery complete.");
        }

        /// <summary>
        /// 進階：簡單重試機制（避免一次失敗就放棄）
        /// </summary>
        public static void RunRetryExample()
        {
            var restarter = new GNetworkAdapterRestarter();

            var options = new GNetworkAdapterRestartOptions
            {
                AdapterName = "乙太網路 2"
            };

            int maxRetry = 3;

            for (int i = 0; i < maxRetry; i++)
            {
                Console.WriteLine("Try restart network... attempt: " + (i + 1));

                var result = restarter.Restart(options);

                if (result.Success)
                {
                    Console.WriteLine("Success.");
                    return;
                }

                Console.WriteLine("Failed: " + result.Message);
                System.Threading.Thread.Sleep(3000);
            }

            Console.WriteLine("All retry failed.");
        }
    }
}