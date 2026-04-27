using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Sunny.UI;

namespace GUtility.Common.Progress
{
    /// <summary>
    /// 通用進度視窗。
    /// Form Title 與 Message 由使用端決定。
    /// Designer 元件：
    /// - lbl_Message
    /// - pb_Progress
    /// </summary>
    public partial class GProgressForm : UIForm
    {
        public GProgressForm()
            : this("Loading", "Preparing...")
        {
        }

        public GProgressForm(string title, string message)
        {
            InitializeComponent();

            InitializeProgressForm(title, message);
        }

        private void InitializeProgressForm(string title, string message)
        {
            SetTitle(title);
            SetMessage(message);

            // SunnyUI 3.9.6.0 的 UIProcessBar 不一定有 Minimum 屬性
            // 只設定 Maximum / Value 較穩
            pb_Progress.Maximum = 100;
            pb_Progress.Value = 0;
            pb_Progress.ShowValue = true;
        }

        /// <summary>
        /// 設定視窗 Title。
        /// </summary>
        public void SetTitle(string title)
        {
            SafeInvoke(delegate
            {
                Text = string.IsNullOrEmpty(title) ? "Loading" : title;
            });
        }

        /// <summary>
        /// 設定目前訊息。
        /// </summary>
        public void SetMessage(string message)
        {
            SafeInvoke(delegate
            {
                lbl_Message.Text = string.IsNullOrEmpty(message) ? string.Empty : message;
            });
        }

        /// <summary>
        /// 更新進度與訊息。
        /// </summary>
        public void UpdateProgress(int percent, string message)
        {
            SafeInvoke(delegate
            {
                percent = ClampPercent(percent);

                pb_Progress.Value = percent;
                lbl_Message.Text = message ?? string.Empty;
            });
        }

        /// <summary>
        /// 完成進度。
        /// </summary>
        public void Finish(string message)
        {
            SafeInvoke(delegate
            {
                pb_Progress.Value = 100;
                lbl_Message.Text = string.IsNullOrEmpty(message)
                    ? "Completed."
                    : message;
            });
        }

        /// <summary>
        /// 重置進度。
        /// </summary>
        public void ResetProgress(string message)
        {
            SafeInvoke(delegate
            {
                pb_Progress.Value = 0;
                lbl_Message.Text = string.IsNullOrEmpty(message)
                    ? "Preparing..."
                    : message;
            });
        }

        private int ClampPercent(int percent)
        {
            if (percent < 0)
                return 0;

            if (percent > 100)
                return 100;

            return percent;
        }

        private void SafeInvoke(Action action)
        {
            if (action == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(action);
                return;
            }

            action();
        }
    }
}