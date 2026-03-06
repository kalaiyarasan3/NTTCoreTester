using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core.Models
{
    public class TestSuiteConfig
    {
        public string TestName { get; set; }
        public string Description { get; set; }
        public bool StopOnFailure { get; set; }
        public List<ConfigRequest> Requests { get; set; }
    }

    public class MasterSuite
    {
        public string MasterTestName { get; set; }
        public string Description { get; set; }
        public bool StopOnFailure { get; set; }
        public List<SuiteInfo> Suites { get; set; }
    }

    public class SuiteInfo
    {
        public string TestName { get; set; }
        public string Path { get; set; }
        public bool Enabled { get; set; }
    }
}
