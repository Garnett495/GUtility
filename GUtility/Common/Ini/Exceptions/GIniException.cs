using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Ini.Exceptions
{
    /// <summary>
    /// Ini 相關基底例外。
    /// </summary>
    public class GIniException : Exception
    {
        public GIniException() { }

        public GIniException(string message) : base(message) { }

        public GIniException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
