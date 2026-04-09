using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Runtime.Serialization;

namespace GUtility.Common.Recipe.Exceptions
{
    /// <summary>
    /// Recipe 升版失敗例外。
    /// </summary>
    [Serializable]
    public class GRecipeUpgradeException : GRecipeException
    {
        public GRecipeUpgradeException()
        {
        }

        public GRecipeUpgradeException(string message)
            : base(message)
        {
        }

        public GRecipeUpgradeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GRecipeUpgradeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
