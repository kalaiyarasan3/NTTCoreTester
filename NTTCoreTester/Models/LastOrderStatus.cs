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

        [JsonProperty("cl_ord_id")]
        public string? ClientOrderId { get; set; }

        [JsonProperty("NewClientOrderId")]
        public string? NewClientOrderId { get; set; }

        [JsonProperty("remarks")]
        public string? Remarks { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("qty")]
        public int? Quantity { get; set; }

        [JsonProperty("trantype")]
        public string? TransactionType { get; set; } //Buy/Sell

        [JsonProperty("prd")]
        public string? Product { get; set; } //CNC/MIS

        [JsonProperty("tsym")]
        public string? TypeSymbol { get; set; } //IOC-EQ
    }
}
