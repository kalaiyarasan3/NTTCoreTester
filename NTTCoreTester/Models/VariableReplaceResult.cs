using Newtonsoft.Json.Linq;
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
        private VariableReplaceResult(string value, bool success, string? error)
        {
            Text = value;
            IsSuccess = success;
            Error = error;
        }

        public static VariableReplaceResult Success(string value)
            => new VariableReplaceResult(value, true, null);

        public static VariableReplaceResult Failure(string value, string error)
            => new VariableReplaceResult(value, false, error);

    }
}
