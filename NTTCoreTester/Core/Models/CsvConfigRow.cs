using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core.Models
{
    public class CsvConfigRow
    {
        public string SuiteName { get; set; }
        public string Description { get; set; }
        public string  StopOnFailure { get; set; }
        public string EndPoint { get; set; }
        public string Method { get; set; }
        public string Headers { get; set; }
        public string Payload { get; set; }
        public string Activity { get; set; }

    }
}
