using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.Request
{
    public class CancelOrderRequest
    {
        [JsonProperty("actid")]
        public string Actid { get; set; } = string.Empty;

        [JsonProperty("uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonProperty("ordno")]
        public string Ordno { get; set; } = string.Empty;

        [JsonProperty("cl_ord_id")]
        public string ClOrdId { get; set; } = string.Empty;
    }
}
