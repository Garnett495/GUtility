using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Ini.Exceptions
{
    /// <summary>
    /// Ini 檔案不存在例外。
    /// </summary>
    public class GIniFileNotFoundException : GIniException
    {
        public GIniFileNotFoundException() { }

        public GIniFileNotFoundException(string message) : base(message) { }

        public GIniFileNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
