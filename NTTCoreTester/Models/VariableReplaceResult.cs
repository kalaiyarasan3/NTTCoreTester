using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class VariableReplaceResult
    {
        public string Text { get; set; } = "";
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }

       
    }
    public static class VariableReplaceResultExtension
    {
        public static VariableReplaceResult VariableReplace(this string text, bool isSuccess, string? error = null)
        {
            return new VariableReplaceResult
            {
                Text = text,
                IsSuccess = isSuccess,
                Error = error
            };
        }
    }
}
