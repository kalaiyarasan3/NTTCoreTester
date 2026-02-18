using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Core.Models
{
    public class ApiExecutionResult
    {
        public string Endpoint { get; set; }
        public int StatusCode { get; set; }
        public string ResponseBody { get; set; }
        public JObject Json { get; set; }
        public long ResponseTime { get; set; }

        public JObject DataObject => Json?["ResponceDataObject"] as JObject;
    }


}
