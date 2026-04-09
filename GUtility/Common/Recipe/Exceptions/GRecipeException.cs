using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Serialization;

namespace GUtility.Common.Recipe.Exceptions
{
    /// <summary>
    /// Recipe 模組例外基底類別。
    /// </summary>
    [Serializable]
    public class GRecipeException : Exception
    {
        public GRecipeException()
        {
        }

        public GRecipeException(string message)
            : base(message)
        {
        }

        public GRecipeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GRecipeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
