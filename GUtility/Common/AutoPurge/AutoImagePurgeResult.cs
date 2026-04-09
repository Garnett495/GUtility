using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GUtility.Common.Storage
{
    /// <summary>
    /// 自動清除結果。
    /// </summary>
    public class AutoImagePurgeResult
    {
        public bool IsSuccess { get; set; }
        public bool IsPurged { get; set; }
        public string Message { get; set; }
        public int DeletedFileCount { get; set; }
        public long DeletedBytes { get; set; }
        public List<string> DeletedFiles { get; private set; }

        public AutoImagePurgeResult()
        {
            Message = string.Empty;
            DeletedFiles = new List<string>();
        }
    }
}
