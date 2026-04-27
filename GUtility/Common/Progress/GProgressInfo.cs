using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Progress
{
    /// <summary>
    /// 進度資訊模型。
    /// </summary>
    public class GProgressInfo
    {
        /// <summary>
        /// 視窗標題。
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 進度百分比，範圍 0 ~ 100。
        /// </summary>
        public int Percent { get; set; }

        /// <summary>
        /// 目前進度訊息。
        /// </summary>
        public string Message { get; set; }

        public GProgressInfo()
        {
            Title = "Loading";
            Percent = 0;
            Message = "Preparing...";
        }

        public GProgressInfo(int percent, string message)
            : this("Loading", percent, message)
        {
        }

        public GProgressInfo(string title, int percent, string message)
        {
            Title = string.IsNullOrEmpty(title) ? "Loading" : title;
            Percent = ClampPercent(percent);
            Message = message ?? string.Empty;
        }

        private int ClampPercent(int percent)
        {
            if (percent < 0)
                return 0;

            if (percent > 100)
                return 100;

            return percent;
        }
    }
}
