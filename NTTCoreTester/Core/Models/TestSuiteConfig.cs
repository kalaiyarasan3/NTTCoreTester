using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core.Models
{
    public class TestTestConfig
    {
        public string TestName { get; set; }
        public string Description { get; set; }
        public bool StopOnFailure { get; set; }
        public List<ConfigRequest> Requests { get; set; }
    }

    public class MasterTest
    {
        public string MasterTestName { get; set; }
        public string Description { get; set; }
        public bool StopOnFailure { get; set; }
        public List<TestInfo> Tests { get; set; }
    }

    public class TestInfo
    {
        public string TestName { get; set; }
        public string Path { get; set; }
        public bool Enabled { get; set; }
    }
}
