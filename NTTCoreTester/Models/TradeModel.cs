using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class TradeModel
    {
        [JsonProperty("ClientOrdId")]
        public string ClientOrdId { get; set; } = string.Empty;

        [JsonProperty(nameof(BuySell))]
        public string BuySell { get; set; } = string.Empty; 

        [JsonProperty("TradedQty")]
        public int TradedQty { get; set; }
        public decimal TradePrice { get; set; }
    }
}
