using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class PositionBookModel
    {
        [JsonProperty("tsym")]
        public string Symbol { get; set; } = string.Empty;   // tsym    
        [JsonProperty("prd")]
        public string ProductType { get; set; } = string.Empty; // prd (MIS/CNC)
        [JsonProperty("netqty")]
        public int NetQty { get; set; }
        [JsonProperty("netavgprc")]
        public decimal NetAvgPrice { get; set; }
    }
}
