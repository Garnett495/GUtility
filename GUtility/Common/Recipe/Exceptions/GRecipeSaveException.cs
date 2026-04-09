using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Serialization;

namespace GUtility.Common.Recipe.Exceptions
{
    /// <summary>
    /// Recipe 儲存失敗例外。
    /// </summary>
    [Serializable]
    public class GRecipeSaveException : GRecipeException
    {
        public GRecipeSaveException()
        {
        }

        public GRecipeSaveException(string message)
            : base(message)
        {
        }

        public GRecipeSaveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GRecipeSaveException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
