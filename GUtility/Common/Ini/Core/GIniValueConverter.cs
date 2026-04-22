using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace GUtility.Common.Ini.Core
{
    /// <summary>
    /// Ini 字串值與型別值之間的轉換器。
    /// </summary>
    public static class GIniValueConverter
    {
        /// <summary>
        /// 將字串轉為 int。
        /// </summary>
        public static int ToInt(string value, int defaultValue)
        {
            int result;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result)
                ? result
                : defaultValue;
        }

        /// <summary>
        /// 將字串轉為 double。
        /// </summary>
        public static double ToDouble(string value, double defaultValue)
        {
            double result;
            return double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result)
                ? result
                : defaultValue;
        }

        /// <summary>
        /// 將字串轉為 bool。
        /// 支援 true/false, 1/0, yes/no, on/off。
        /// </summary>
        public static bool ToBool(string value, bool defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            string normalized = value.Trim().ToLowerInvariant();

            switch (normalized)
            {
                case "true":
                case "1":
                case "yes":
                case "y":
                case "on":
                    return true;

                case "false":
                case "0":
                case "no":
                case "n":
                case "off":
                    return false;

                default:
                    return defaultValue;
            }
        }

        /// <summary>
        /// 將字串轉為 enum。
        /// </summary>
        public static TEnum ToEnum<TEnum>(string value, TEnum defaultValue) where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            TEnum result;
            return Enum.TryParse<TEnum>(value.Trim(), true, out result)
                ? result
                : defaultValue;
        }

        /// <summary>
        /// 將值轉為可寫入 Ini 的字串。
        /// </summary>
        public static string ToInvariantString(object value)
        {
            if (value == null)
                return string.Empty;

            IFormattable formattable = value as IFormattable;
            if (formattable != null)
                return formattable.ToString(null, CultureInfo.InvariantCulture);

            return value.ToString();
        }
    }
}
