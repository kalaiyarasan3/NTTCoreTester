using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class TradeModel
    {
        public string ClientOrdId { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int TradedQty { get; set; }
        public decimal TradePrice { get; set; }
    }
}
