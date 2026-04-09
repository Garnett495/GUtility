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
    /// Recipe 基本使用範例。
    /// </summary>
    public class GRecipeBasicUsageExample
    {
        /// <summary>
        /// 執行範例。
        /// </summary>
        public static void Run()
        {
            string recipeFolder = @"D:\Recipes";

            GRecipeService service = new GRecipeService(recipeFolder);

            // 建立新的 Recipe
            GRecipeDocument<GRecipeCameraConfig> recipe =
                service.CreateNew<GRecipeCameraConfig>("Camera_Config_01", GRecipeFormat.Json);

            // 設定資料
            recipe.Data.Exposure = 120;
            recipe.Data.Gain = 1.8;
            recipe.Data.ImageWidth = 2448;
            recipe.Data.ImageHeight = 2048;

            // 儲存
            service.Save(recipe);

            // 載入
            GRecipeDocument<GRecipeCameraConfig> loaded =
                service.Load<GRecipeCameraConfig>("Camera_Config_01");

            Console.WriteLine("=== Basic Usage Example ===");
            Console.WriteLine("RecipeName : " + loaded.RecipeName);
            Console.WriteLine("Format     : " + loaded.Format);
            Console.WriteLine("Exposure   : " + loaded.Data.Exposure);
            Console.WriteLine("Gain       : " + loaded.Data.Gain);
            Console.WriteLine("Width      : " + loaded.Data.ImageWidth);
            Console.WriteLine("Height     : " + loaded.Data.ImageHeight);
            Console.WriteLine();
        }
    }
}
