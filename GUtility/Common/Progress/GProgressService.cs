using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;


namespace GUtility.Common.Progress
{
    /// <summary>
    /// 通用進度視窗服務。
    /// 負責顯示、更新、完成、關閉 GProgressForm。
    /// </summary>
    public class GProgressService : IDisposable
    {
        private GProgressForm _form;
        private Thread _uiThread;
        private ManualResetEvent _formReadyEvent;

        private bool _isShown;
        private bool _disposed;

        /// <summary>
        /// 是否已顯示進度視窗。
        /// </summary>
        public bool IsShown
        {
            get { return _isShown; }
        }

        public GProgressService()
        {
            _formReadyEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// 顯示進度視窗。
        /// </summary>
        public void Show(string title, string message)
        {
            if (_disposed)
                throw new ObjectDisposedException("GProgressService");

            if (_isShown)
                return;

            _isShown = true;
            _formReadyEvent.Reset();

            _uiThread = new Thread(delegate ()
            {
                _form = new GProgressForm(title, message);
                _formReadyEvent.Set();

                Application.Run(_form);
            });

            _uiThread.IsBackground = true;
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.Start();

            _formReadyEvent.WaitOne();
        }

        /// <summary>
        /// 顯示進度視窗。
        /// </summary>
        public void Show(GProgressInfo info)
        {
            if (info == null)
                info = new GProgressInfo();

            Show(info.Title, info.Message);
            Report(info.Percent, info.Message);
        }

        /// <summary>
        /// 更新進度。
        /// </summary>
        public void Report(int percent, string message)
        {
            if (!_isShown || _form == null)
                return;

            _form.UpdateProgress(percent, message);
        }

        /// <summary>
        /// 更新進度。
        /// </summary>
        public void Report(GProgressInfo info)
        {
            if (info == null)
                return;

            if (!_isShown)
                Show(info.Title, info.Message);

            Report(info.Percent, info.Message);
        }

        /// <summary>
        /// 設定視窗標題。
        /// </summary>
        public void SetTitle(string title)
        {
            if (!_isShown || _form == null)
                return;

            _form.SetTitle(title);
        }

        /// <summary>
        /// 設定訊息文字。
        /// </summary>
        public void SetMessage(string message)
        {
            if (!_isShown || _form == null)
                return;

            _form.SetMessage(message);
        }

        /// <summary>
        /// 完成進度。
        /// </summary>
        public void Finish(string message)
        {
            if (!_isShown || _form == null)
                return;

            _form.Finish(message);
        }

        /// <summary>
        /// 關閉進度視窗。
        /// </summary>
        public void Close()
        {
            if (!_isShown || _form == null)
                return;

            try
            {
                if (_form.InvokeRequired)
                {
                    _form.BeginInvoke(new Action(delegate
                    {
                        _form.Close();
                    }));
                }
                else
                {
                    _form.Close();
                }
            }
            catch
            {
                // 關閉階段不拋例外，避免影響主流程。
            }
            finally
            {
                _form = null;
                _isShown = false;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Close();

            if (_formReadyEvent != null)
            {
                _formReadyEvent.Dispose();
                _formReadyEvent = null;
            }

            _disposed = true;
        }
    }
}