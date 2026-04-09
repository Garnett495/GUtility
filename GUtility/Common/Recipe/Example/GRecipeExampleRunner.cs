using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Recipe.Example
{
    /// <summary>
    /// Recipe 範例執行入口。
    /// </summary>
    public class GRecipeExampleRunner
    {
        /// <summary>
        /// 執行全部範例。
        /// </summary>
        public static void RunAll()
        {
            try
            {
                GRecipeBasicUsageExample.Run();
                GRecipeCopyExample.Run();
                GRecipeCompareExample.Run();

                Console.WriteLine("All recipe examples finished.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Example execution failed.");
                Console.WriteLine(ex.Message);
            }
        }
    }
}