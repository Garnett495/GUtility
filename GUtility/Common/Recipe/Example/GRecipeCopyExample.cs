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
    /// Recipe 複製範例。
    /// </summary>
    public class GRecipeCopyExample
    {
        /// <summary>
        /// 執行範例。
        /// </summary>
        public static void Run()
        {
            string recipeFolder = @"D:\Recipes";

            GRecipeService service = new GRecipeService(recipeFolder);

            // 先建立一份來源 Recipe
            GRecipeDocument<GRecipeCameraConfig> source =
                service.CreateNew<GRecipeCameraConfig>("Camera_Source", GRecipeFormat.Json);

            source.Data.Exposure = 100;
            source.Data.Gain = 1.5;
            source.Data.ImageWidth = 1920;
            source.Data.ImageHeight = 1080;

            service.Save(source);

            // 同格式複製
            GRecipeDocument<GRecipeCameraConfig> copied1 =
                service.Copy<GRecipeCameraConfig>("Camera_Source", "Camera_Copy_Json");

            // 跨格式複製
            GRecipeDocument<GRecipeCameraConfig> copied2 =
                service.Copy<GRecipeCameraConfig>(
                    "Camera_Source",
                    GRecipeFormat.Json,
                    "Camera_Copy_Xml",
                    GRecipeFormat.Xml);

            Console.WriteLine("=== Copy Example ===");
            Console.WriteLine("Source Recipe  : Camera_Source");
            Console.WriteLine("Copy Recipe 1  : " + copied1.RecipeName + " (" + copied1.Format + ")");
            Console.WriteLine("Copy Recipe 2  : " + copied2.RecipeName + " (" + copied2.Format + ")");
            Console.WriteLine();
        }
    }
}
