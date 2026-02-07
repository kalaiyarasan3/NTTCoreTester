using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.Request
{
    public class ModifyOrderRequest
    {
        [JsonProperty("exch")]
        public string Exch { get; set; } = "NSE";

        [JsonProperty("ordno")]
        public string Ordno { get; set; } = string.Empty;

        [JsonProperty("prctyp")]
        public string Prctyp { get; set; } = "LMT";

        [JsonProperty("prc")]
        public string Prc { get; set; } = "0";

        [JsonProperty("qty")]
        public int Qty { get; set; } = 0;

        [JsonProperty("tsym")]
        public string Tsym { get; set; } = string.Empty;

        [JsonProperty("ret")]
        public string Ret { get; set; } = "";

        [JsonProperty("mkt_protection")]
        public string MktProtection { get; set; } = "0";

        [JsonProperty("trgprc")]
        public string Trgprc { get; set; } = "0.00";

        [JsonProperty("dscqty")]
        public int Dscqty { get; set; } = 0;

        [JsonProperty("ext_remarks")]
        public string ExtRemarks { get; set; } = "";

        [JsonProperty("cl_ord_id")]
        public string ClOrdId { get; set; } = string.Empty;

        [JsonProperty("uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string Actid { get; set; } = string.Empty;

        [JsonProperty("bpprc")]
        public string Bpprc { get; set; } = "0";

        [JsonProperty("blprc")]
        public string Blprc { get; set; } = "0";

        [JsonProperty("trailprc")]
        public string Trailprc { get; set; } = "0";

        [JsonProperty("ordersource")]
        public string Ordersource { get; set; } = "Web";

        [JsonProperty("orderactivity")]
        public int Orderactivity { get; set; } = 2;

        [JsonProperty("prd")]
        public string Prd { get; set; } = "CNC";

        [JsonProperty("tqty")]
        public int Tqty { get; set; } = 0;
    }
}
