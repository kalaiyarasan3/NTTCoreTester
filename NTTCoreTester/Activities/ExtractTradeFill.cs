using Newtonsoft.Json.Linq;
using NTTCoreTester.Core;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractTradeFill(PlaceholderCache cache) : IActivityHandler
    {
        private readonly PlaceholderCache _cache = cache;

        public string Name => nameof(ExtractTradeFill);

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

                int totalSignedQty = relatedTrades.Sum(t =>
                {
                    int qty = t["TradedQty"]?.Value<int>() ?? 0;
                    string side = t["BuySell"]?.Value<string>() ?? "";

                    return side.Equals("Buy", StringComparison.OrdinalIgnoreCase)
                        ? qty
                        : -qty;
                });

                if (totalSignedQty == 0)
                    return "No quantity filled yet".FailWithLog(false);              

                _cache.Set(Constants.FilledQty, totalSignedQty);

                var log= $"Client order id: {clientOrdId}: Total filled quantity = {totalSignedQty}";
                log.Warn();

                return ActivityResult.Success(log);
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"Error extracting trade fill: {ex.Message}");
            }
        }
    }
}