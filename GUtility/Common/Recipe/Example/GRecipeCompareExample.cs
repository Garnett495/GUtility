using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Recipe.Application;
using GUtility.Common.Recipe.Core;

namespace GUtility.Common.Recipe.Example
{
    /// <summary>
    /// Recipe 比較範例。
    /// </summary>
    public class GRecipeCompareExample
    {
        /// <summary>
        /// 執行範例。
        /// </summary>
        public static void Run()
        {
            string recipeFolder = @"D:\Recipes";

            GRecipeService service = new GRecipeService(recipeFolder);

            // 建立第一份 Recipe
            GRecipeDocument<GRecipeCameraConfig> recipeA =
                service.CreateNew<GRecipeCameraConfig>("Compare_A", GRecipeFormat.Json);

            recipeA.Data.Exposure = 100;
            recipeA.Data.Gain = 1.5;
            recipeA.Data.ImageWidth = 1920;
            recipeA.Data.ImageHeight = 1080;

            service.Save(recipeA);

            // 建立第二份 Recipe
            GRecipeDocument<GRecipeCameraConfig> recipeB =
                service.CreateNew<GRecipeCameraConfig>("Compare_B", GRecipeFormat.Json);

            recipeB.Data.Exposure = 150;
            recipeB.Data.Gain = 1.5;
            recipeB.Data.ImageWidth = 2448;
            recipeB.Data.ImageHeight = 2048;

            service.Save(recipeB);

            // 比較
            List<GRecipeDifference> differences =
                service.Compare<GRecipeCameraConfig>("Compare_A", "Compare_B");

            Console.WriteLine("=== Compare Example ===");

            if (differences.Count == 0)
            {
                Console.WriteLine("No differences found.");
            }
            else
            {
                int i;
                for (i = 0; i < differences.Count; i++)
                {
                    Console.WriteLine(differences[i].ToString());
                }
            }

            Console.WriteLine();
        }
    }
}
