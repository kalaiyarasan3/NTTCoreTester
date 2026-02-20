using NTTCoreTester.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core.Models
{
    public class ActivityResult
    {
        public bool IsSuccess { get; set; }
        public bool ContinueExecution { get; set; } = true;
        public string Message { get; set; } = "";
        public static ActivityResult Success(string message = "") => new()
        { IsSuccess = true, ContinueExecution = true, Message = message };

        public static ActivityResult SoftFail(string message) =>
            new ActivityResult { IsSuccess = false, ContinueExecution = true, Message = message };

        public static ActivityResult HardFail(string message) =>
            new ActivityResult { IsSuccess = false, ContinueExecution = false, Message = message };
        

    }
    public static class ActivityResultExtesnion
    {
        public static ActivityResult Result(this bool isSuccess, bool continueExecution, string message = "")
        {
            return new ActivityResult
            {
                IsSuccess = isSuccess,
                ContinueExecution = continueExecution,
                Message = message
            };
        }
        public static ActivityResult FailWithLog(this string message, bool hardFail = true)
        {
            message.Error();
            return hardFail
                ? ActivityResult.HardFail(message)
                : ActivityResult.SoftFail(message);
        }
       
    }

}
