using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUtility.Common.Storage;

public class MainController
{
    private AutoImagePurgeService _autoImagePurgeService;

    public void Init()
    {
        AutoImagePurgeOptions options = new AutoImagePurgeOptions();
        options.TargetFolder = @"D:\AOIImages";
        options.StartPurgeUsedPercent = 80;
        options.StopPurgeUsedPercent = 70;
        options.CheckIntervalSeconds = 30;
        options.IncludeSubdirectories = true;
        options.MinimumFileAgeSeconds = 30;

        _autoImagePurgeService = new AutoImagePurgeService(options);
        _autoImagePurgeService.Start();
    }

    public void ManualPurgeTest()
    {
        if (_autoImagePurgeService == null)
            return;

        AutoImagePurgeResult result = _autoImagePurgeService.ExecuteNow();

        Console.WriteLine("IsSuccess = " + result.IsSuccess);
        Console.WriteLine("IsPurged = " + result.IsPurged);
        Console.WriteLine("Message = " + result.Message);
        Console.WriteLine("DeletedFileCount = " + result.DeletedFileCount);
        Console.WriteLine("DeletedBytes = " + result.DeletedBytes);
    }

    public void Close()
    {
        if (_autoImagePurgeService != null)
        {
            _autoImagePurgeService.Dispose();
            _autoImagePurgeService = null;
        }
    }
}
