using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTTCoreTester.Activities
{
    public class ExtractPledgeSymbol(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ExtractPledgeSymbol);

        public async Task<ActivityResult> Execute(ApiExecutionResult result, string endpoint, string payLoad)
        {
            try
            {
                var holdings = GetHoldings(result);

                if (holdings == null || !holdings.Any())
                    return "Holdings not found".FailWithLog();

                var holding = holdings.FirstOrDefault(x => x.HoldQuantity >= 1);

                if (holding == null)
                    return "No holding with quantity greater than 5 found".FailWithLog(false);

                var log = $"Symbol {holding.ExchangeData.Symbol}, holding qty: {holding.HoldQuantity}";
                log.Warn();

                cache.Set(Constants.PledgeOrderSymbol, holding.ExchangeData.Symbol);

                return ActivityResult.Success(log);
            }
            catch (Exception ex)
            {
                return $"Error extracting pledge symbol: {ex.Message}".FailWithLog();
            }
        }
        private List<HoldingDetails>? GetHoldings(ApiExecutionResult result)
        {
            var holdingsArray = result.DataObject?["Holdings"];

            if (holdingsArray == null || holdingsArray.Type != JTokenType.Array)
                return null;

            return holdingsArray.ToObject<List<HoldingDetails>>();
        }
    }
}
