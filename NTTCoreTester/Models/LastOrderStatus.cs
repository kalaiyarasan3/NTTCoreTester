using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    class LastOrderStatus
    {
        [JsonProperty("AllOrders")]
        public List<OrderDetails> AllOrders { get; set; }
    }

    class OrderDetails
    {
        [JsonProperty("ordno")]
        public string? OrderNumber { get; set; }

        [JsonProperty("OrderId")]
        public int OrderId { get; set; }

        [JsonProperty("cl_ord_id")]
        public string? ClientOrderId { get; set; }

        [JsonProperty("NewClientOrderId")]
        public string? NewClientOrderId { get; set; }

        [JsonProperty("OriginalClientOrderId")]
        public string? OriginalClientOrderId { get; set; }

        [JsonProperty("remarks")]
        public string? Remarks { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("exchsts")]
        public string? ExchangeStatus { get; set; }

        [JsonProperty("qty")]
        public int? Quantity { get; set; }

        [JsonProperty("prc")]
        public string? Price { get; set; }

        [JsonProperty("trantype")]
        public string? TransactionType { get; set; }

        [JsonProperty("prd")]
        public string? Product { get; set; }

        [JsonProperty("tsym")]
        public string? TypeSymbol { get; set; }

        [JsonProperty("orderactivity")]
        public int OrderActivity { get; set; }

        [JsonProperty("ordenttm")]
        public string? OrderEntryTime { get; set; }

        [JsonProperty("AddedOn")]
        public string? AddedOn { get; set; }

        [JsonProperty("rejreason")]
        public string? RejectionReason { get; set; }
    }
}
