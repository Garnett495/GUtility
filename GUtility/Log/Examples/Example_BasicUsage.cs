using System;
using GUtility.Log;

namespace GUtility.Examples
{
    /// <summary>
    /// 最基本的使用範例。
    /// </summary>
    public class Example_BasicUsage
    {
        public static void Run()
        {
            GLogConfig config = new GLogConfig();
            config.RootFolder = "Logs";
            config.MinimumLevel = GLogLevel.Debug;
            config.RetentionDays = 30;
            config.MaxQueueCount = 10000;
            config.FileName = "System.log.txt";

            GLog.Init(config);

            GLog.Info("System", "System started.");
            GLog.Debug("Camera", "Camera initialization begin.");
            GLog.Warn("PLC", "PLC response is slow.");
            GLog.Info("Recipe", "Recipe loaded successfully.");

            GLog.Flush();
            GLog.Shutdown();

            Console.WriteLine("Basic log example finished.");
        }
    }
}