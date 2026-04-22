using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Ini.Models
{
    /// <summary>
    /// Ini Key 資訊模型。
    /// </summary>
    public class GIniKeyInfo
    {
        /// <summary>
        /// Section 名稱。
        /// </summary>
        public string Section { get; set; }

        /// <summary>
        /// Key 名稱。
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Value 字串值。
        /// </summary>
        public string Value { get; set; }
    }
}