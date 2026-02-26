using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Models
{
    public class OrderMarginDetails// GetOrderMargin response model
    {
        public decimal AvailableMargin { get; set; }
        public decimal OrderMargin { get; set; }
        public decimal MarginUsedPrev { get; set; }
        public decimal Charges { get; set; }
    }
}
