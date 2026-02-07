using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.Request
{
    public class OrderMarginRequest
    {
        [JsonProperty("uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string Actid { get; set; } = string.Empty;

        [JsonProperty("exch")]
        public string Exch { get; set; } = "NSE";

        [JsonProperty("tsym")]
        public string Tsym { get; set; } = string.Empty;

        [JsonProperty("qty")]
        public string Qty { get; set; } = "0";  // string in your example (even though numeric)

        [JsonProperty("trantype")]
        public string Trantype { get; set; } = "Buy";

        [JsonProperty("ordno")]
        public string Ordno { get; set; } = "0";

        [JsonProperty("prc")]
        public decimal Prc { get; set; } = 0;

        [JsonProperty("trgprc")]
        public string Trgprc { get; set; } = "0";

        [JsonProperty("dscqty")]
        public string Dscqty { get; set; } = "0";

        [JsonProperty("prd")]
        public string Prd { get; set; } = "CNC";

        [JsonProperty("prctyp")]
        public string Prctyp { get; set; } = "Limit";

        [JsonProperty("ordersource")]
        public string Ordersource { get; set; } = "Web";

        [JsonProperty("blprc")]
        public string Blprc { get; set; } = "0";

        [JsonProperty("filshares")]
        public string Filshares { get; set; } = "0";

        [JsonProperty("rorgqty")]
        public string Rorgqty { get; set; } = "0";

        [JsonProperty("orgtrgprc")]
        public string Orgtrgprc { get; set; } = "0";

        [JsonProperty("snonum")]
        public string Snonum { get; set; } = "0";
    }
}
