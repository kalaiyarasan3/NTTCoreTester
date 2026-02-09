using Newtonsoft.Json;

namespace NTTCoreTester.Application.Features.Orders.ViewModels
{
    public class SingleOrdHistResponse
    {
        [JsonProperty("OrderRequestId")]
        public string? OrderRequestId { get; set; }

        [JsonProperty("reconcilation")]
        public bool Reconcilation { get; set; }

        [JsonProperty("ActivityId")]
        public int ActivityId { get; set; }

        [JsonProperty("OrderId")]
        public int OrderId { get; set; }

        [JsonProperty("cl_ord_id")]
        public string ClOrdId { get; set; } = string.Empty;

        [JsonProperty("OriginalClientOrderId")]
        public string OriginalClientOrderId { get; set; } = string.Empty;

        [JsonProperty("NewClientOrderId")]
        public string NewClientOrderId { get; set; } = string.Empty;

        [JsonProperty("uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonProperty("actid")]
        public string ActId { get; set; } = string.Empty;

        [JsonProperty("pro")]
        public bool Pro { get; set; }

        [JsonProperty("exch")]
        public string Exch { get; set; } = string.Empty;

        [JsonProperty("Segment")]
        public string Segment { get; set; } = string.Empty;

        [JsonProperty("tsym")]
        public string Tsym { get; set; } = string.Empty;

        [JsonProperty("prc")]
        public decimal Prc { get; set; }

        [JsonProperty("qty")]
        public int Qty { get; set; }

        [JsonProperty("fillshares")]
        public int FillShares { get; set; }

        [JsonProperty("trantype")]
        public string TranType { get; set; } = string.Empty;

        [JsonProperty("prctyp")]
        public string PrcType { get; set; } = string.Empty;

        [JsonProperty("prd")]
        public string Prd { get; set; } = string.Empty;

        [JsonProperty("ordno")]
        public string OrdNo { get; set; } = string.Empty;

        [JsonProperty("remarks")]
        public string Remarks { get; set; } = string.Empty;
    }
}
