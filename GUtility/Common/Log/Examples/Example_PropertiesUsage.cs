using System;
using System.Collections.Generic;
using GUtility.Common.Log;

namespace GUtility.Examples
{
    /// <summary>
    /// 帶有額外屬性的記錄範例。
    /// </summary>
    public class Example_PropertiesUsage
    {
        public static void Run()
        {
            GLogConfig config = new GLogConfig();
            config.RootFolder = "Logs";
            config.MinimumLevel = GLogLevel.Debug;
            config.RetentionDays = 30;
            config.FileName = "System.log.txt";

            GLog.Init(config);

            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties["RecipeName"] = "Glass_A01";
            properties["CCDNo"] = "2";
            properties["LotId"] = "LOT20260408";
            properties["Result"] = "NG";

            GLog.Info("Inspection", "Inspection completed.", properties);

            GLog.Flush();
            GLog.Shutdown();

            Console.WriteLine("Properties log example finished.");
        }
    }
}