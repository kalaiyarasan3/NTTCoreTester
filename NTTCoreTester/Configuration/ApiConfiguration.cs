using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Configuration
{
    public class ApiConfiguration
    {
        public string BaseUrl { get; set; }
        public Dictionary<string, string> DefaultHeaders { get; set; }
        public string Site { get; set; }
        public int OtpTimeout { get; set; }
        public int MaxResponseTime { get; set; }
    }

    public class ReportConfig
    {
        public string OutputFolder { get; set; }
        public string FilePrefix { get; set; }
    }
}
