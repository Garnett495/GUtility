using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using GUtility.Common.Recipe.Core;

namespace GUtility.Common.Recipe.Comparison
{
    /// <summary>
    /// Recipe 比較工具。
    /// </summary>
    public class GRecipeComparer
    {
        /// <summary>
        /// 比較兩個 RecipeDocument。
        /// </summary>
        public List<GRecipeDifference> Compare<T>(
            GRecipeDocument<T> left,
            GRecipeDocument<T> right,
            GCompareOptions options = null)
            where T : class, new()
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");

            if (options == null)
                options = GCompareOptions.Default();

            List<GRecipeDifference> differences = new List<GRecipeDifference>();

            // 比 Metadata（可選）
            if (!options.IgnoreMetadata)
            {
                CompareObject(left, right, "Root", differences, options);
            }

            // 比 Data（重點）
            CompareObject(left.Data, right.Data, "Data", differences, options);

            return differences;
        }

        /// <summary>
        /// 比較兩個一般物件。
        /// </summary>
        private void CompareObject(
            object left,
            object right,
            string path,
            List<GRecipeDifference> diffs,
            GCompareOptions options)
        {
            // null 處理
            if (left == null && right == null)
                return;

            if (left == null || right == null)
            {
                AddDiff(path, left, right, diffs);
                return;
            }

            Type type = left.GetType();

            // 基本型別
            if (IsSimpleType(type))
            {
                if (!IsValueEqual(left, right, options))
                {
                    AddDiff(path, left, right, diffs);
                }
                return;
            }

            // 只比 public property
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanRead)
                    continue;

                string childPath = path + "." + prop.Name;

                object leftValue = prop.GetValue(left, null);
                object rightValue = prop.GetValue(right, null);

                // 判斷是否為基本型別
                if (IsSimpleType(prop.PropertyType))
                {
                    if (!IsValueEqual(leftValue, rightValue, options))
                    {
                        AddDiff(childPath, leftValue, rightValue, diffs);
                    }
                }
                else
                {
                    // 遞迴
                    if (options.RecursiveCompare)
                    {
                        CompareObject(leftValue, rightValue, childPath, diffs, options);
                    }
                }
            }
        }

        /// <summary>
        /// 判斷是否為基本型別。
        /// </summary>
        private bool IsSimpleType(Type type)
        {
            return
                type.IsPrimitive ||
                type.IsEnum ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime);
        }

        /// <summary>
        /// 判斷值是否相等。
        /// </summary>
        private bool IsValueEqual(object left, object right, GCompareOptions options)
        {
            // null 處理
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
            {
                // null vs "" 特例
                if (options.TreatNullAndEmptyStringAsEqual)
                {
                    string l = left as string;
                    string r = right as string;

                    if ((l == null && r == "") || (l == "" && r == null))
                        return true;
                }

                return false;
            }

            // string 特別處理
            if (left is string && right is string)
            {
                string l = (string)left;
                string r = (string)right;

                if (options.IgnoreStringCase)
                    return string.Equals(l, r, StringComparison.OrdinalIgnoreCase);

                return l == r;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// 加入差異。
        /// </summary>
        private void AddDiff(
            string path,
            object left,
            object right,
            List<GRecipeDifference> diffs)
        {
            diffs.Add(new GRecipeDifference(
                path,
                ToSafeString(left),
                ToSafeString(right)));
        }

        /// <summary>
        /// 安全轉字串。
        /// </summary>
        private string ToSafeString(object obj)
        {
            if (obj == null)
                return "null";

            return obj.ToString();
        }
    }
}
