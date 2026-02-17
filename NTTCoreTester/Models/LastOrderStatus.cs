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

        [JsonProperty("remarks")]
        public string? Remarks { get; set; }

        [JsonProperty("status")]
        public string? Status { get; set; }
    }
}
