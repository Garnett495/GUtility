using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace GUtility.Common.Network
{
    /// <summary>
    /// Windows 網路介面卡重啟工具。
    /// 
    /// 適用場景：
    /// 1. GigE 相機斷線
    /// 2. 網卡資源被占住
    /// 3. 相機 Close / Dispose 後仍無法重新連線
    /// 
    /// 注意：
    /// 執行程式必須具備系統管理員權限。
    /// </summary>
    public class GNetworkAdapterRestarter
    {
        /// <summary>
        /// 重啟指定網路介面卡。
        /// </summary>
        /// <param name="options">網卡重啟設定。</param>
        /// <returns>重啟結果。</returns>
        public GNetworkAdapterRestartResult Restart(GNetworkAdapterRestartOptions options)
        {
            if (options == null)
                return GNetworkAdapterRestartResult.Fail("Options is null.");

            if (string.IsNullOrWhiteSpace(options.AdapterName))
                return GNetworkAdapterRestartResult.Fail("AdapterName is empty.");

            try
            {
                Disable(options.AdapterName);

                Thread.Sleep(options.DisableWaitMs);

                Enable(options.AdapterName);

                Thread.Sleep(options.EnableWaitMs);

                return GNetworkAdapterRestartResult.Ok();
            }
            catch (Exception ex)
            {
                return GNetworkAdapterRestartResult.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 停用指定網卡。
        /// </summary>
        /// <param name="adapterName">網路介面名稱。</param>
        private void Disable(string adapterName)
        {
            RunNetsh(
                string.Format("interface set interface name=\"{0}\" admin=disable", adapterName)
            );
        }

        /// <summary>
        /// 啟用指定網卡。
        /// </summary>
        /// <param name="adapterName">網路介面名稱。</param>
        private void Enable(string adapterName)
        {
            RunNetsh(
                string.Format("interface set interface name=\"{0}\" admin=enable", adapterName)
            );
        }

        /// <summary>
        /// 執行 netsh 指令。
        /// </summary>
        /// <param name="arguments">netsh 參數。</param>
        private void RunNetsh(string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "netsh";
            psi.Arguments = arguments;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.Verb = "runas";

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (process.ExitCode != 0)
                {
                    throw new Exception(error + output);
                }
            }
        }
    }
}
