using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Application.Features.Orders.Request
{
    public class AddMarketWatchSymbolsRequest
    {
        public long Wlid { get; set; } = 78318;
        public string Scrips { get; set; } = "NSE|18102";
    } 
}
