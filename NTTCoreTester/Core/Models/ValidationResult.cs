using NTTCoreTester.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core.Models
{
    public class ValidationResult
    {
        public bool IsSuccess { get; set; }
        public string BusinessStatus { get; set; }
        public HTTPEnumStatus StatusCode { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

}
