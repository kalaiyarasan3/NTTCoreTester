using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class ActivityResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "";
    }
    public static class ActivityResultExtesnion
    {
         public static ActivityResult Result(this bool isSuccess, string message = "")
        {
            return new ActivityResult
            {
                IsSuccess = isSuccess,
                Message = message
            };
        }
    }

}
