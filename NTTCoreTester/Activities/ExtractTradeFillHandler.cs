using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractTradeFillHandler(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => "ExtractTradeFill";

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var tradesArray = result.DataObject?["AllTrades"] as JArray;

                if (tradesArray == null)
                    return ActivityResult.HardFail("Trades not found");

                string? clientOrdId = _cache.Get<string>(Constants.ClientOrdId);
                if (clientOrdId == null)
                    return $"Client order Id not found in {endpoint}".FailWithLog();

                var relatedTrades = tradesArray
                    .Where(t => t["ClientOrdId"]?.Value<string>() == clientOrdId);

                int totalFilledQty = relatedTrades
                    .Sum(t => t["TradedQty"]?.Value<int>() ?? 0);

                if (totalFilledQty == 0)
                    return "No quantity filled yet".FailWithLog(false);

                _cache.Set(Constants.FilledQty, totalFilledQty);

                return ActivityResult.Success();
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"Error extracting trade fill: {ex.Message}");
            }
        }
    }
}