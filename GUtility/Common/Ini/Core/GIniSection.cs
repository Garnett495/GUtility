using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Ini.Core
{
    /// <summary>
    /// 表示 Ini 中的一個 Section。
    /// </summary>
    public class GIniSection
    {
        /// <summary>
        /// Section 名稱。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Key / Value 集合。
        /// </summary>
        public Dictionary<string, string> Values { get; private set; }

        public GIniSection(string name, StringComparer keyComparer)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            if (keyComparer == null)
                throw new ArgumentNullException("keyComparer");

            Name = name;
            Values = new Dictionary<string, string>(keyComparer);
        }
    }
}
