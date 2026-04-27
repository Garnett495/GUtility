using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace GUtility.Common.Progress.Example
{
    /// <summary>
    /// GProgressService 使用範例。
    /// </summary>
    public static class GProgressExample
    {
        /// <summary>
        /// 模擬系統初始化流程。
        /// </summary>
        public static void RunInitializeExample()
        {
            using (GProgressService progress = new GProgressService())
            {
                progress.Show("System Initializing", "Preparing...");

                progress.Report(10, "Loading machine config...");
                Thread.Sleep(300);

                progress.Report(30, "Loading recipe...");
                Thread.Sleep(300);

                progress.Report(50, "Connecting camera...");
                Thread.Sleep(300);

                progress.Report(70, "Connecting IO module...");
                Thread.Sleep(300);

                progress.Report(90, "Initializing inspection runner...");
                Thread.Sleep(300);

                progress.Finish("Initialize completed.");
                Thread.Sleep(300);
            }
        }

        /// <summary>
        /// 使用 GProgressInfo 更新進度。
        /// </summary>
        public static void RunProgressInfoExample()
        {
            using (GProgressService progress = new GProgressService())
            {
                progress.Show(new GProgressInfo(
                    "Recipe Loading",
                    0,
                    "Preparing recipe..."));

                Thread.Sleep(300);

                progress.Report(new GProgressInfo(
                    "Recipe Loading",
                    40,
                    "Reading recipe file..."));

                Thread.Sleep(300);

                progress.Report(new GProgressInfo(
                    "Recipe Loading",
                    80,
                    "Applying recipe parameters..."));

                Thread.Sleep(300);

                progress.Finish("Recipe loaded.");
                Thread.Sleep(300);
            }
        }
    }
}
