using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using GUtility.Common.Ini.Abstractions;
using GUtility.Common.Ini.Exceptions;
using GUtility.Common.Ini.Models;

namespace GUtility.Common.Ini.Core
{
    /// <summary>
    /// Ini 檔案操作主類別。
    /// 以記憶體快取方式管理資料，必要時再 Save() 寫回硬碟。
    /// </summary>
    public class GIniFile : IGIniFile
    {
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, GIniSection> _sections;
        private readonly StringComparer _sectionComparer;
        private readonly StringComparer _keyComparer;

        /// <summary>
        /// Ini 檔實際路徑。
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// 目前操作選項。
        /// </summary>
        public GIniOptions Options { get; private set; }

        /// <summary>
        /// 建立 IniFile 實例。
        /// </summary>
        public GIniFile(string filePath)
            : this(filePath, null)
        {
        }

        /// <summary>
        /// 建立 IniFile 實例。
        /// </summary>
        public GIniFile(string filePath, GIniOptions options)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath");

            FilePath = filePath;
            Options = options ?? new GIniOptions();

            _sectionComparer = Options.IgnoreSectionCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            _keyComparer = Options.IgnoreKeyCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

            _sections = new Dictionary<string, GIniSection>(_sectionComparer);

            EnsureFileExistsIfNeeded();
            Reload();
        }

        /// <summary>
        /// 重新載入檔案內容到記憶體。
        /// </summary>
        public void Reload()
        {
            lock (_syncRoot)
            {
                _sections.Clear();

                if (!File.Exists(FilePath))
                    throw new GIniFileNotFoundException("Ini file not found: " + FilePath);

                string[] lines = File.ReadAllLines(FilePath, Encoding.UTF8);
                GIniSection currentSection = null;

                for (int i = 0; i < lines.Length; i++)
                {
                    string rawLine = lines[i];

                    if (rawLine == null)
                        continue;

                    string line = rawLine.Trim();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.StartsWith(";") || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        string sectionName = line.Substring(1, line.Length - 2).Trim();

                        if (string.IsNullOrWhiteSpace(sectionName))
                            throw new GIniParseException("Section name is empty at line " + (i + 1) + ".");

                        currentSection = GetOrCreateSectionInternal(sectionName);
                        continue;
                    }

                    int equalIndex = line.IndexOf('=');
                    if (equalIndex < 0)
                    {
                        // 沒有 '=' 的行視為格式錯誤
                        throw new GIniParseException("Invalid key/value format at line " + (i + 1) + ": " + rawLine);
                    }

                    string key = line.Substring(0, equalIndex).Trim();
                    string value = line.Substring(equalIndex + 1).Trim();

                    if (string.IsNullOrWhiteSpace(key))
                        throw new GIniParseException("Key is empty at line " + (i + 1) + ".");

                    if (currentSection == null)
                    {
                        // 若沒有 section，放到預設 section
                        currentSection = GetOrCreateSectionInternal("General");
                    }

                    currentSection.Values[key] = value;
                }
            }
        }

        /// <summary>
        /// 將記憶體資料儲存到硬碟。
        /// </summary>
        public void Save()
        {
            lock (_syncRoot)
            {
                string directory = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                StringBuilder sb = new StringBuilder();

                bool isFirstSection = true;
                foreach (KeyValuePair<string, GIniSection> sectionPair in _sections)
                {
                    if (!isFirstSection)
                        sb.AppendLine();

                    sb.AppendLine("[" + sectionPair.Key + "]");

                    foreach (KeyValuePair<string, string> valuePair in sectionPair.Value.Values)
                    {
                        sb.AppendLine(valuePair.Key + "=" + (valuePair.Value ?? string.Empty));
                    }

                    isFirstSection = false;
                }

                File.WriteAllText(FilePath, sb.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// 判斷 Section 是否存在。
        /// </summary>
        public bool SectionExists(string section)
        {
            ValidateSectionName(section);

            lock (_syncRoot)
            {
                return _sections.ContainsKey(section);
            }
        }

        /// <summary>
        /// 判斷 Key 是否存在。
        /// </summary>
        public bool KeyExists(string section, string key)
        {
            ValidateSectionName(section);
            ValidateKeyName(key);

            lock (_syncRoot)
            {
                GIniSection iniSection;
                if (!_sections.TryGetValue(section, out iniSection))
                    return false;

                return iniSection.Values.ContainsKey(key);
            }
        }

        /// <summary>
        /// 刪除指定 Key。
        /// </summary>
        public void RemoveKey(string section, string key)
        {
            ValidateSectionName(section);
            ValidateKeyName(key);

            lock (_syncRoot)
            {
                GIniSection iniSection;
                if (_sections.TryGetValue(section, out iniSection))
                {
                    iniSection.Values.Remove(key);

                    if (Options.AutoSave)
                        Save();
                }
            }
        }

        /// <summary>
        /// 刪除指定 Section。
        /// </summary>
        public void RemoveSection(string section)
        {
            ValidateSectionName(section);

            lock (_syncRoot)
            {
                _sections.Remove(section);

                if (Options.AutoSave)
                    Save();
            }
        }

        /// <summary>
        /// 讀取字串。
        /// </summary>
        public string ReadString(string section, string key, string defaultValue = "")
        {
            ValidateSectionName(section);
            ValidateKeyName(key);

            lock (_syncRoot)
            {
                GIniSection iniSection;
                if (!_sections.TryGetValue(section, out iniSection))
                    return defaultValue;

                string value;
                if (!iniSection.Values.TryGetValue(key, out value))
                    return defaultValue;

                return value;
            }
        }

        /// <summary>
        /// 讀取整數。
        /// </summary>
        public int ReadInt(string section, string key, int defaultValue = 0)
        {
            string value = ReadString(section, key, null);
            return GIniValueConverter.ToInt(value, defaultValue);
        }

        /// <summary>
        /// 讀取雙精度浮點數。
        /// </summary>
        public double ReadDouble(string section, string key, double defaultValue = 0d)
        {
            string value = ReadString(section, key, null);
            return GIniValueConverter.ToDouble(value, defaultValue);
        }

        /// <summary>
        /// 讀取布林值。
        /// </summary>
        public bool ReadBool(string section, string key, bool defaultValue = false)
        {
            string value = ReadString(section, key, null);
            return GIniValueConverter.ToBool(value, defaultValue);
        }

        /// <summary>
        /// 讀取列舉值。
        /// </summary>
        public TEnum ReadEnum<TEnum>(string section, string key, TEnum defaultValue) where TEnum : struct
        {
            string value = ReadString(section, key, null);
            return GIniValueConverter.ToEnum<TEnum>(value, defaultValue);
        }

        /// <summary>
        /// 寫入字串。
        /// </summary>
        public void WriteString(string section, string key, string value)
        {
            ValidateSectionName(section);
            ValidateKeyName(key);

            lock (_syncRoot)
            {
                GIniSection iniSection = GetOrCreateSectionInternal(section);
                iniSection.Values[key] = value ?? string.Empty;

                if (Options.AutoSave)
                    Save();
            }
        }

        /// <summary>
        /// 寫入整數。
        /// </summary>
        public void WriteInt(string section, string key, int value)
        {
            WriteString(section, key, GIniValueConverter.ToInvariantString(value));
        }

        /// <summary>
        /// 寫入雙精度浮點數。
        /// </summary>
        public void WriteDouble(string section, string key, double value)
        {
            WriteString(section, key, GIniValueConverter.ToInvariantString(value));
        }

        /// <summary>
        /// 寫入布林值。
        /// </summary>
        public void WriteBool(string section, string key, bool value)
        {
            WriteString(section, key, value ? "true" : "false");
        }

        /// <summary>
        /// 寫入列舉值。
        /// </summary>
        public void WriteEnum<TEnum>(string section, string key, TEnum value) where TEnum : struct
        {
            WriteString(section, key, value.ToString());
        }

        /// <summary>
        /// 取得或建立 Section。
        /// </summary>
        private GIniSection GetOrCreateSectionInternal(string section)
        {
            GIniSection iniSection;
            if (!_sections.TryGetValue(section, out iniSection))
            {
                iniSection = new GIniSection(section, _keyComparer);
                _sections.Add(section, iniSection);
            }

            return iniSection;
        }

        /// <summary>
        /// 若設定允許，建立不存在的檔案。
        /// </summary>
        private void EnsureFileExistsIfNeeded()
        {
            if (File.Exists(FilePath))
                return;

            if (!Options.AutoCreateIfMissing)
                throw new GIniFileNotFoundException("Ini file not found: " + FilePath);

            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(FilePath, string.Empty, Encoding.UTF8);
        }

        /// <summary>
        /// 驗證 Section 名稱。
        /// </summary>
        private static void ValidateSectionName(string section)
        {
            if (string.IsNullOrWhiteSpace(section))
                throw new ArgumentNullException("section");
        }

        /// <summary>
        /// 驗證 Key 名稱。
        /// </summary>
        private static void ValidateKeyName(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");
        }
    }
}