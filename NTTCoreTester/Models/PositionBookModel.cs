using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class PositionBookModel
    {
        public string Symbol { get; set; } = string.Empty;   // tsym    
        public string ProductType { get; set; } = string.Empty; // prd (MIS/CNC)
        public int NetQty { get; set; }
        public decimal NetAvgPrice { get; set; }
    }
}
