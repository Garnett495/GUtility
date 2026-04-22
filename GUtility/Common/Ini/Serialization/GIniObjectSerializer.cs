using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using GUtility.Common.Ini.Abstractions;

namespace GUtility.Common.Ini.Serialization
{
    /// <summary>
    /// 簡易 Ini 物件序列化器。
    /// 規則：
    /// 1. Section 使用類別名稱
    /// 2. Key 使用屬性名稱
    /// 3. 支援 string/int/double/bool/enum
    /// </summary>
    public class GIniObjectSerializer : IGIniSerializer
    {
        /// <summary>
        /// 將物件寫入 Ini。
        /// </summary>
        public void WriteObject<T>(IGIniFile iniFile, T instance) where T : class, new()
        {
            if (iniFile == null)
                throw new ArgumentNullException("iniFile");

            if (instance == null)
                throw new ArgumentNullException("instance");

            Type type = typeof(T);
            string section = type.Name;

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (!property.CanRead)
                    continue;

                object value = property.GetValue(instance, null);
                Type propertyType = property.PropertyType;

                if (propertyType == typeof(string))
                {
                    iniFile.WriteString(section, property.Name, value == null ? string.Empty : value.ToString());
                }
                else if (propertyType == typeof(int))
                {
                    iniFile.WriteInt(section, property.Name, value == null ? 0 : (int)value);
                }
                else if (propertyType == typeof(double))
                {
                    iniFile.WriteDouble(section, property.Name, value == null ? 0d : (double)value);
                }
                else if (propertyType == typeof(bool))
                {
                    iniFile.WriteBool(section, property.Name, value != null && (bool)value);
                }
                else if (propertyType.IsEnum)
                {
                    if (value != null)
                    {
                        MethodInfo method = typeof(IGIniFile).GetMethod("WriteEnum").MakeGenericMethod(propertyType);
                        method.Invoke(iniFile, new object[] { section, property.Name, value });
                    }
                }
            }

            iniFile.Save();
        }

        /// <summary>
        /// 從 Ini 讀取物件。
        /// </summary>
        public T ReadObject<T>(IGIniFile iniFile) where T : class, new()
        {
            if (iniFile == null)
                throw new ArgumentNullException("iniFile");

            T instance = new T();
            Type type = typeof(T);
            string section = type.Name;

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (!property.CanWrite)
                    continue;

                Type propertyType = property.PropertyType;

                if (propertyType == typeof(string))
                {
                    string value = iniFile.ReadString(section, property.Name, string.Empty);
                    property.SetValue(instance, value, null);
                }
                else if (propertyType == typeof(int))
                {
                    int value = iniFile.ReadInt(section, property.Name, 0);
                    property.SetValue(instance, value, null);
                }
                else if (propertyType == typeof(double))
                {
                    double value = iniFile.ReadDouble(section, property.Name, 0d);
                    property.SetValue(instance, value, null);
                }
                else if (propertyType == typeof(bool))
                {
                    bool value = iniFile.ReadBool(section, property.Name, false);
                    property.SetValue(instance, value, null);
                }
                else if (propertyType.IsEnum)
                {
                    object defaultValue = Activator.CreateInstance(propertyType);
                    MethodInfo method = typeof(IGIniFile).GetMethod("ReadEnum").MakeGenericMethod(propertyType);
                    object value = method.Invoke(iniFile, new object[] { section, property.Name, defaultValue });
                    property.SetValue(instance, value, null);
                }
            }

            return instance;
        }
    }
}
