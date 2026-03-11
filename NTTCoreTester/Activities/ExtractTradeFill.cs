using Newtonsoft.Json.Linq;
using NTTCoreTester.Core.Helper;
using NTTCoreTester.Core.Models;
using NTTCoreTester.Models;

namespace NTTCoreTester.Activities
{
    public class ExtractTradeFill(PlaceholderCache _cache) : IActivityHandler
    {
        public string Name => nameof(ExtractTradeFill);

        public ActivityResult Execute(ApiExecutionResult result, string endpoint)
        {
            try
            {
                var tradesToken = result.DataObject?["AllTrades"];

                if (tradesToken == null || tradesToken.Type != JTokenType.Array)
                    return ActivityResult.HardFail("Trades not found");

                var tradesArray = tradesToken.ToObject<List<TradeModel>>();
                if (tradesArray == null || !tradesArray.Any())
                    return "Trades list empty".FailWithLog(true);

                var clientOrderIds = _cache.Get<Dictionary<string, string>>(Constants.ClientOrdIds);

                if (clientOrderIds == null || !clientOrderIds.Any())
                    return "ClientOrdIds not found in cache".FailWithLog(true);

                var filledQtyMap = new Dictionary<string, int>();

                foreach (var kv in clientOrderIds)
                {
                    string key = kv.Key;
                    string ordId = kv.Value;

                    var relatedTrades = tradesArray.Where(t => t.ClientOrdId == ordId);

                    int signedQty = relatedTrades.Sum(t =>
                    {
                        int qty = t.TradedQty;
                        return t.BuySell.Equals("Buy", StringComparison.OrdinalIgnoreCase)
                            ? qty
                            : -qty;
                    });

                    filledQtyMap[key] = signedQty;

                    $"Trade fill: {key} Qty:{signedQty}".Warn();
                }

                _cache.Set(Constants.FilledQtyBySymbol, filledQtyMap);

                return ActivityResult.Success("Trade fills extracted");
            }
            catch (Exception ex)
            {
                return ActivityResult.HardFail($"Error extracting trade fill: {ex.Message}");
            }
        }
    }
}