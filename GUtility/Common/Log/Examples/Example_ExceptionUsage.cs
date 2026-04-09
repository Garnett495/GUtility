using System;
using GUtility.Common.Log;

namespace GUtility.Examples
{
    /// <summary>
    /// 例外記錄範例。
    /// </summary>
    public class Example_ExceptionUsage
    {
        public static void Run()
        {
            GLogConfig config = new GLogConfig();
            config.RootFolder = "Logs";
            config.MinimumLevel = GLogLevel.Debug;
            config.RetentionDays = 30;
            config.FileName = "System.log.txt";

            GLog.Init(config);

            try
            {
                int a = 0;
                int b = 100 / a;
            }
            catch (Exception ex)
            {
                GLog.Error("Math", "Divide by zero error.", ex);
            }

            GLog.Flush();
            GLog.Shutdown();

            Console.WriteLine("Exception log example finished.");
        }
    }
}