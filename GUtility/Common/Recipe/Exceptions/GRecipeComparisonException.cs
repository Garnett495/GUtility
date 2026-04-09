using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Serialization;

namespace GUtility.Common.Recipe.Exceptions
{
    /// <summary>
    /// Recipe 比較失敗例外。
    /// </summary>
    [Serializable]
    public class GRecipeComparisonException : GRecipeException
    {
        public GRecipeComparisonException()
        {
        }

        public GRecipeComparisonException(string message)
            : base(message)
        {
        }

        public GRecipeComparisonException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GRecipeComparisonException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
