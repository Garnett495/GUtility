using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Ini.Exceptions
{
    /// <summary>
    /// Ini 解析失敗例外。
    /// </summary>
    public class GIniParseException : GIniException
    {
        public GIniParseException() { }

        public GIniParseException(string message) : base(message) { }

        public GIniParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
