using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.Request
{
    [JsonObject(MemberSerialization.OptIn)]  
    public class PlaceOrderRequest
    {
        [JsonProperty("uid")]
        public string Uid { get; set; } = "90255961";

        [JsonProperty("actid")]
        public string Actid { get; set; } = "90255961";

        [JsonProperty("exch")]
        public string Exch { get; set; } = "NSE";

        [JsonProperty("trantype")]
        public string Trantype { get; set; } = "Buy";

        [JsonProperty("norenordno")]
        public string Norenordno { get; set; } = "0";

        [JsonProperty("segment")]
        public string Segment { get; set; } = "EQ";

        [JsonProperty("tsym")]
        public string Tsym { get; set; } = string.Empty;

        [JsonProperty("qty")]
        public int Qty { get; set; } = 0;

        [JsonProperty("prc")]
        public string Prc { get; set; } = "0";

        [JsonProperty("trgprc")]
        public string Trgprc { get; set; } = "0";

        [JsonProperty("dscqty")]
        public int Dscqty { get; set; } = 0;

        [JsonProperty("prd")]
        public string Prd { get; set; } = "CNC";

        [JsonProperty("prctyp")]
        public string Prctyp { get; set; } = "Limit";

        [JsonProperty("mkt_protection")]
        public string MktProtection { get; set; } = "0";

        [JsonProperty("ret")]
        public string Ret { get; set; } = "DAY";

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = "";

        [JsonProperty("ordersource")]
        public string Ordersource { get; set; } = "Web";

        [JsonProperty("bpprc")]
        public string Bpprc { get; set; } = "0";

        [JsonProperty("blprc")]
        public string Blprc { get; set; } = "0";

        [JsonProperty("trailprc")]
        public string Trailprc { get; set; } = "0";

        [JsonProperty("ext_remarks")]
        public string ExtRemarks { get; set; } = "External remarks";

        [JsonProperty("cl_ord_id")]
        public string ClOrdId { get; set; } = "";

        [JsonProperty("tsym2")]
        public string Tsym2 { get; set; } = "";

        [JsonProperty("trantype2")]
        public string Trantype2 { get; set; } = "";

        [JsonProperty("qty2")]
        public string Qty2 { get; set; } = "0";

        [JsonProperty("prc2")]
        public string Prc2 { get; set; } = "0";

        [JsonProperty("tsym3")]
        public string Tsym3 { get; set; } = "0";

        [JsonProperty("trantype3")]
        public string Trantype3 { get; set; } = "";

        [JsonProperty("qty3")]
        public string Qty3 { get; set; } = "0";

        [JsonProperty("prc3")]
        public string Prc3 { get; set; } = "0";

        [JsonProperty("algo_id")]
        public string AlgoId { get; set; } = "0";

        [JsonProperty("naic_code")]
        public string NaicCode { get; set; } = "0";

        [JsonProperty("snonum")]
        public string Snonum { get; set; } = "0";

        [JsonProperty("filshares")]
        public string Filshares { get; set; } = "0";

        [JsonProperty("rorgqty")]
        public string Rorgqty { get; set; } = "0";

        [JsonProperty("orgtrgprc")]
        public string Orgtrgprc { get; set; } = "0";
    }
}