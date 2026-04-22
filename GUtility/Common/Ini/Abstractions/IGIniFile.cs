using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace GUtility.Common.Ini.Abstractions
{
    /// <summary>
    /// Ini 檔案操作介面。
    /// </summary>
    public interface IGIniFile
    {
        /// <summary>
        /// Ini 檔實際路徑。
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// 取得目前選項設定。
        /// </summary>
        Models.GIniOptions Options { get; }

        /// <summary>
        /// 重新載入檔案內容到記憶體。
        /// </summary>
        void Reload();

        /// <summary>
        /// 將記憶體中的資料存回檔案。
        /// </summary>
        void Save();

        /// <summary>
        /// 判斷 Section 是否存在。
        /// </summary>
        bool SectionExists(string section);

        /// <summary>
        /// 判斷 Key 是否存在。
        /// </summary>
        bool KeyExists(string section, string key);

        /// <summary>
        /// 刪除指定 Key。
        /// </summary>
        void RemoveKey(string section, string key);

        /// <summary>
        /// 刪除指定 Section。
        /// </summary>
        void RemoveSection(string section);

        /// <summary>
        /// 讀取字串。
        /// </summary>
        string ReadString(string section, string key, string defaultValue = "");

        /// <summary>
        /// 讀取整數。
        /// </summary>
        int ReadInt(string section, string key, int defaultValue = 0);

        /// <summary>
        /// 讀取雙精度浮點數。
        /// </summary>
        double ReadDouble(string section, string key, double defaultValue = 0d);

        /// <summary>
        /// 讀取布林值。
        /// </summary>
        bool ReadBool(string section, string key, bool defaultValue = false);

        /// <summary>
        /// 讀取列舉值。
        /// </summary>
        TEnum ReadEnum<TEnum>(string section, string key, TEnum defaultValue) where TEnum : struct;

        /// <summary>
        /// 寫入字串。
        /// </summary>
        void WriteString(string section, string key, string value);

        /// <summary>
        /// 寫入整數。
        /// </summary>
        void WriteInt(string section, string key, int value);

        /// <summary>
        /// 寫入雙精度浮點數。
        /// </summary>
        void WriteDouble(string section, string key, double value);

        /// <summary>
        /// 寫入布林值。
        /// </summary>
        void WriteBool(string section, string key, bool value);

        /// <summary>
        /// 寫入列舉值。
        /// </summary>
        void WriteEnum<TEnum>(string section, string key, TEnum value) where TEnum : struct;
    }
}