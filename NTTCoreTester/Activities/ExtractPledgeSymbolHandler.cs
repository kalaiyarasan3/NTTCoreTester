using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
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
    public class ExtractPledgeSymbolHandler(PlaceholderCache cache) : IActivityHandler
    {
        public string Name => nameof(ExtractPledgeSymbolHandler);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var holdings = GetHoldings(result);

                if (holdings == null || !holdings.Any())
                    return "Holdings not found".FailWithLog();

                var holding = holdings.FirstOrDefault(x => x.HoldQuantity >= 5);

                if (holding == null)
                    return "No holding with quantity greater than 5 found".FailWithLog(false);

                $"Symbol {holding.ExchangeData.Symbol}, holding qty: {holding.HoldQuantity}".Warn();

                cache.Set(Constants.PledgeOrderSymbol, holding.ExchangeData.Symbol);

                return ActivityResult.Success();
            }
            catch (Exception)
            {

                throw;
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
