using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core
{
    public class TestSuiteConfig
    {
        public string SuiteName { get; set; }
        public string Description { get; set; }
        public bool StopOnFailure { get; set; }
        public List<ConfigRequest> Requests { get; set; }
    }
}
