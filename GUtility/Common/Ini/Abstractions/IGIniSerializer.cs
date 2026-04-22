using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUtility.Common.Ini.Abstractions
{
    /// <summary>
    /// 物件與 Ini 之間的序列化介面。
    /// </summary>
    public interface IGIniSerializer
    {
        /// <summary>
        /// 將物件寫入 Ini。
        /// </summary>
        void WriteObject<T>(IGIniFile iniFile, T instance) where T : class, new();

        /// <summary>
        /// 從 Ini 讀取物件。
        /// </summary>
        T ReadObject<T>(IGIniFile iniFile) where T : class, new();
    }
}
