using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.Request
{
    public class GetSecurityInfoRequest
    {
        [JsonProperty("exch")]
        public string Exch { get; set; } = "NSE";

        [JsonProperty("token")]
        public string Token { get; set; } = string.Empty;
    }
}
