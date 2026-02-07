using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.Request
{ 
    public class AddMarketWatchNameRequest
    {
        public string MarketWatchId { get; set; } = "0";
        public int IsDeleted { get; set; } = 1;
        public string MarketWatchName { get; set; } = string.Empty;
    }
}