using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Serialization;

namespace GUtility.Common.Recipe.Exceptions
{
    /// <summary>
    /// Recipe 載入失敗例外。
    /// </summary>
    [Serializable]
    public class GRecipeLoadException : GRecipeException
    {
        public GRecipeLoadException()
        {
        }

        public GRecipeLoadException(string message)
            : base(message)
        {
        }

        public GRecipeLoadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GRecipeLoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}